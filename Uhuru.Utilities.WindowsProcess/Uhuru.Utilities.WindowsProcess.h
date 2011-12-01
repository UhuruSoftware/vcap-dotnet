// Uhuru.Utilities.WindowsProcess.h

#pragma once
#include "Stdafx.h"
#include "WinNT.h"
#include <stddef.h>
#include <vcclr.h>
#include <iostream>
#include <map>
#include <string>

using namespace std;
using namespace System::Runtime::InteropServices;

#if PSAPI_VERSION == 1
#pragma comment(lib,"psapi.lib")
#endif

#define MAX_PROCESS_COUNT 512

#define PROCESS_PARAMS_OFFSET	(offsetof(PEB,ProcessParameters))
#define CMD_LINE_OFFSET			(offsetof(RTL_USER_PROCESS_PARAMETERS,CommandLine))

#define FILETIME_TO_INT64(filetime) (((__int64)(filetime.dwHighDateTime) << 32) | (__int64)filetime.dwLowDateTime)

#define FILETIME_ADD(ft1,ft2) (FILETIME_TO_INT64(ft1) + FILETIME_TO_INT64(ft2)) 

typedef struct _PROCESS_BASIC_INFORMATION
{
	NTSTATUS ExitStatus;
	PVOID PebBaseAddress; // here it is
	ULONG_PTR AffinityMask;
	DWORD BasePriority;
	HANDLE UniqueProcessId;
	HANDLE InheritedFromUniqueProcessId;
} PROCESS_BASIC_INFORMATION, *PPROCESS_BASIC_INFORMATION;


// this is the type that holds a pointer to the command line in Buffer
typedef struct _UNICODE_STRING
{
	USHORT Length;
	USHORT MaximumLength;
	PWSTR Buffer;
} UNICODE_STRING, *PUNICODE_STRING;


typedef struct _RTL_USER_PROCESS_PARAMETERS
{
	ULONG MaximumLength;
	ULONG Length;
	ULONG Flags;
	ULONG DebugFlags;
	PVOID ConsoleHandle;
	ULONG ConsoleFlags;
	HANDLE StdInputHandle;
	HANDLE StdOutputHandle;
	HANDLE StdErrorHandle;
	UNICODE_STRING CurrentDirectoryPath;
	HANDLE CurrentDirectoryHandle;
	UNICODE_STRING DllPath;
	UNICODE_STRING ImagePathName;
	UNICODE_STRING CommandLine;
	//	................... etc
}RTL_USER_PROCESS_PARAMETERS;

typedef struct _PEB {
	CHAR	filling[4];
	HANDLE  Mutant;
	PVOID   ImageBaseAddress;
	PVOID	LoaderData;
	PVOID	ProcessParameters;
	//	................... etc
} PEB;

template<typename T>
struct Value
{
	Value(const T& _val){
		val = _val;
	}
	T val;
};

typedef NTSTATUS 
	(NTAPI *NtQueryInformationProcess)(
	IN HANDLE ProcessHandle,
	IN DWORD ProcessInformationClass,
	OUT PVOID ProcessInformationBuffer,
	IN ULONG ProcessInformationLength,
	OUT PULONG ReturnLength
	);

typedef map<wstring,int> ColIndexes;
typedef map<int,string>  ValueMap;

using namespace System;

namespace Uhuru 
{
	namespace Utilities
	{
		namespace WindowsProcess {

			public ref struct ProcessInformationEntry
			{
			public:
				int Workset;
				String^ CommandLine;
				int CPU;
				int ParentProcess;
				String^ User;
				int ProcessId;

				int GetWorkset()
				{
					return Workset;
				}
			};

			public ref class ProcessInformation
			{

			private:

				static NtQueryInformationProcess pfnQueryProcessInfo = (NtQueryInformationProcess)GetFunctionAddress();

				static const char* GetProcessCommandLine( HANDLE ProcessHandle )
				{
					if( !ProcessHandle )
						return NULL;

					static char commandLineBufferA[1024];
					WCHAR	commandLineBufferW[1024];

					ULONG ulRet1 = 0;
					SIZE_T ulRet = 0;

					PROCESS_BASIC_INFORMATION processBasicInfo= { 0 };
					PVOID pPebAddress = 0;
					LPVOID processParams = 0;
					UNICODE_STRING cmdLine;

					NTSTATUS ret = pfnQueryProcessInfo(ProcessHandle, 0, &processBasicInfo, sizeof(processBasicInfo), &ulRet1);

					pPebAddress = (processBasicInfo.PebBaseAddress)?processBasicInfo.PebBaseAddress:(PVOID)0x7efde000;

					if( !ReadProcessMemory(ProcessHandle, (INT8*)pPebAddress + PROCESS_PARAMS_OFFSET, &processParams, sizeof(LPVOID), &ulRet) )
						return NULL;

					if( !ReadProcessMemory(ProcessHandle, (INT8*)processParams + CMD_LINE_OFFSET, &cmdLine, sizeof(UNICODE_STRING), &ulRet) )
						return NULL;

					if( !ReadProcessMemory(ProcessHandle,cmdLine.Buffer,commandLineBufferW,cmdLine.Length, &ulRet) )
						return NULL;

					commandLineBufferW[ulRet/2] = L'\0';

					int cbWritten = WideCharToMultiByte(CP_ACP,0,commandLineBufferW,cmdLine.Length,commandLineBufferA,1024,NULL,NULL);

					if( !cbWritten )
						return NULL;

					return commandLineBufferA;
				}

				static const char* GetProcessExecutableName( HANDLE ProcessHandle )
				{
					static char exeName[MAX_PATH];
					char szImageName[MAX_PATH];

					if( GetProcessImageFileNameA(ProcessHandle,szImageName,MAX_PATH) )
					{
						char *tmpName = strrchr(szImageName,L'\\')+1;
						sprintf(exeName,"%s",tmpName);
					}
					else
						sprintf(exeName,"%-32s",L"<unknown>");

					return exeName;
				}

				static BOOL EnableDebugPrivileges()
				{
					HANDLE hToken;
					LUID luidSeDebugNameValue;
					TOKEN_PRIVILEGES tokenPrivileges;

					if( !OpenProcessToken(GetCurrentProcess(),TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken ) )
						return FALSE;

					if( !LookupPrivilegeValue(NULL,SE_DEBUG_NAME,&luidSeDebugNameValue) )
						return FALSE;

					tokenPrivileges.PrivilegeCount = 1; 
					tokenPrivileges.Privileges[0].Luid = luidSeDebugNameValue; 
					tokenPrivileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED; 

					if( !AdjustTokenPrivileges(hToken,FALSE,&tokenPrivileges,0,NULL,NULL) ){
						CloseHandle(hToken);
						return FALSE;
					}


					CloseHandle(hToken);

					return TRUE;
				}

				static LPVOID GetFunctionAddress()
				{
					HMODULE hDll;

					hDll = LoadLibraryA("ntdll.dll");

					if( !hDll ){
						printf("LoadLibrary() failed with code %d.\n",GetLastError());
						ExitProcess(1);
					}

					pfnQueryProcessInfo = (NtQueryInformationProcess)GetProcAddress(hDll,"ZwQueryInformationProcess");

					if( !pfnQueryProcessInfo )
					{
						printf("GetProcAddress() failed with code %d.\n",GetLastError());
						FreeLibrary(hDll);
						ExitProcess(1);
					}

					FreeLibrary(hDll);

					return (LPVOID)pfnQueryProcessInfo;
				}

				static const char*	GetProcessUsername(HANDLE ProcessHandle, BOOL bIncDomain)  
				{ 
					static char domainAccountName[300]; 
					char userName[256], domainName[256], tokenBuf[256]; 
					HANDLE hToken = 0; 
					TOKEN_USER *ptu; 
					DWORD userLen = 256, domainLen = 256; 
					int iNameUse; 

					// Open the token associated with the process
					if (!OpenProcessToken(ProcessHandle,TOKEN_QUERY,&hToken))
						return NULL; 

					// Get the user SID associated with this token
					ptu = (TOKEN_USER*)tokenBuf; 
					if (!GetTokenInformation(hToken,(TOKEN_INFORMATION_CLASS)1,ptu,300,&userLen)) 
						return NULL; 

					// Lookup the (domain\)account name of the SID
					if (!LookupAccountSidA(0, ptu->User.Sid, userName, &userLen, domainName, &domainLen, (PSID_NAME_USE)&iNameUse)) 
						return NULL; 

					if (domainLen && bIncDomain) { 
						strcpy(domainAccountName,domainName); 
						strcat(domainAccountName,"\\"); 
						strcat(domainAccountName,userName); 
					} else { 
						strcpy(domainAccountName,userName); 
					} 

					CloseHandle(hToken); 
					return domainAccountName;
				} 

				static DWORD GetProcessMemInfo( HANDLE ProcessHandle )
				{
					PROCESS_MEMORY_COUNTERS pmc;

					pmc.cb = sizeof(PROCESS_MEMORY_COUNTERS);

					BOOL bRet = GetProcessMemoryInfo(ProcessHandle,&pmc,sizeof(pmc));

					if( !bRet )
						return -1;

					return (DWORD)pmc.WorkingSetSize / 1024;
				}

				static DWORD GetProcessParrentPid( HANDLE ProcessHandle )
				{
					PROCESS_BASIC_INFORMATION pbi;
					DWORD parentPid = -1;

					NTSTATUS ntRet = pfnQueryProcessInfo(ProcessHandle,0,&pbi,sizeof(pbi),0);

					if( ntRet )
						return 0;

					return (DWORD)pbi.InheritedFromUniqueProcessId;
				}

			public:

				static array<ProcessInformationEntry^>^ GetProcessInformation(bool getWorkset, bool getCmd, bool getCPU, bool getParentProcess, bool getUser, bool getPid, int processId)
				{
					DWORD		ProcessIDs[MAX_PROCESS_COUNT];
					DWORD		procCpuUsage[MAX_PROCESS_COUNT];

					__int64		perProcessTimes[2][MAX_PROCESS_COUNT];
					__int64		sysTimes[2];


					_FILETIME	sysTimeKernel, sysTimeUser,sysTimeIdle;
					_FILETIME	procTimeCreation, procTimeExit, procTimeKernel, procTimeUser;

					DWORD		bytesReturned;
					BOOL		bRet;

					EnableDebugPrivileges();

					bRet = EnumProcesses(ProcessIDs,sizeof(DWORD) * MAX_PROCESS_COUNT, &bytesReturned);

					DWORD processCount = bytesReturned / sizeof(DWORD);

					const char* userName = "";
					const char* cmdLine = "";
					int			Pid = 0;
					int			cpu = 0;			
					DWORD		mem = 0;
					DWORD		parentPid = 0;
					BOOL		hasFilter = FALSE;

					if(processId != 0)
					{
						hasFilter = TRUE;
					}

					const int RUN_COUNT = 2;
					HANDLE hProcess;

					array<ProcessInformationEntry^>^ result = gcnew array<ProcessInformationEntry^>(processCount);

					for (int i=0; i<processCount; i++)
					{
						result[i] = gcnew ProcessInformationEntry();
					}

					for(int k = 0; k < RUN_COUNT; k++)
					{
						GetSystemTimes(&sysTimeIdle,&sysTimeKernel,&sysTimeUser);
						sysTimes[k] = FILETIME_ADD(sysTimeKernel,sysTimeUser);

						for(int i = 0; i < processCount; i++ )
						{

							

							if(hasFilter && ProcessIDs[i] != processId)
							{
								continue;
							}

							hProcess = OpenProcess(PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION | PROCESS_VM_READ,FALSE, ProcessIDs[i]);

							if( !hProcess )
							{
								continue;
							}

							GetProcessTimes(hProcess, &procTimeCreation, &procTimeExit, &procTimeKernel, &procTimeUser);
							perProcessTimes[k][i] = FILETIME_ADD(procTimeKernel, procTimeUser);

							// get CPU usage for each process ( cpuUsage = (deltaProcessTime * 100) / deltaSystemTime
							// deltaProcessTime = currentProcessTime - prevProcessTime
							// deltaSystemTime = currentSystemTime - prevSystemTime
							if (k)
							{ 
								__int64 deltaProcTime	= perProcessTimes[k][i] - perProcessTimes[k-1][i];
								__int64 deltaSysTime	= sysTimes[k] - sysTimes[k-1];

								procCpuUsage[i] = (deltaProcTime * 100) / deltaSysTime;
							}

							// get the command line/user name/pid for the current process ( only once )
							if( k )
							{
								ValueMap	values;


								if(getUser)
								{
									userName = GetProcessUsername(hProcess,TRUE);
									result[i]->User = userName != NULL ? Marshal::PtrToStringAnsi(IntPtr((char*)userName)) : "";
									userName = "sloboz";
								}

								if(getCmd) 
								{
									cmdLine	= GetProcessCommandLine( hProcess );
									result[i]->CommandLine = cmdLine != NULL ? Marshal::PtrToStringAnsi(IntPtr((char*)cmdLine)) : "";
								}

								if(getPid)
								{
									Pid = ProcessIDs[i];
									result[i]->ProcessId = Pid;
								}

								if(getWorkset)
								{
									mem = GetProcessMemInfo( hProcess );
									result[i]->Workset = mem;
								}

								if(getParentProcess)
								{
									parentPid = GetProcessParrentPid(hProcess);
									result[i]->ParentProcess = parentPid;
								}

								if(getCPU)
								{
									result[i]->CPU = procCpuUsage[i];
								}
							}
							CloseHandle(hProcess);

						}
						SleepEx(250,FALSE);
					}

					return result;
				};
			};
		}
	}
}