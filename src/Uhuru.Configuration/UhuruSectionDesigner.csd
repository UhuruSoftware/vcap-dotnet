﻿<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d" namespace="Uhuru.Configuration" xmlSchemaNamespace="urn:Uhuru.Configuration" assemblyName="Uhuru.Configuration" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
  </typeDefinitions>
  <configurationElements>
    <configurationSection name="UhuruSection" namespace="Uhuru.Configuration" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="uhuru">
      <elementProperties>
        <elementProperty name="DEA" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="dea" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/DEAElement" />
          </type>
        </elementProperty>
        <elementProperty name="Service" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="service" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/ServiceElement" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="DEAElement">
      <attributeProperties>
        <attributeProperty name="BaseDir" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="baseDir" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="LocalRoute" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="localRoute" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="FilerPort" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="filerPort" isReadOnly="false" defaultValue="12345">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="StatusPort" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="statusPort" isReadOnly="false" defaultValue="0">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="HeartbeatInterval" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="heartBeatInterval" isReadOnly="false" defaultValue="10000">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="MessageBus" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="messageBus" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Multitenant" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="multiTenant" isReadOnly="false" defaultValue="true">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxMemory" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxMemory" isReadOnly="false" defaultValue="2048">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="Secure" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="secure" isReadOnly="false" defaultValue="true">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="EnforceUsageLimit" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="enforceUlimit" isReadOnly="false" defaultValue="true">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="DisableDirCleanup" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="disableDirCleanup" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="ForceHttpSharing" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="forceHttpSharing" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="UseDiskQuota" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="useDiskQuota" isReadOnly="false" defaultValue="true">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxConcurrentStarts" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxConcurrentStarts" isReadOnly="false" defaultValue="3">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Runtimes" isRequired="false" isKey="false" isDefaultCollection="true" xmlName="runtimes" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/RuntimeCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="MSSqlElement">
      <attributeProperties>
        <attributeProperty name="Host" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="host" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="User" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="user" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Password" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="password" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Port" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="port" isReadOnly="false" defaultValue="1433">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxDBSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxDbSize" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int64" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxLengthyQuery" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxLongQuery" isReadOnly="false" defaultValue="3">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxLengthTX" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxLongTx" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxUserConnections" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxUserConns" isReadOnly="false" defaultValue="20">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="LogicalStorageUnits" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="logicalStorageUnits" isReadOnly="false" defaultValue="&quot;C&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="InitialDataSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="initialDataSize" isReadOnly="false" defaultValue="&quot;100MB&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="InitialLogSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="initialLogSize" isReadOnly="false" defaultValue="&quot;50MB&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxDataSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxDataSize" isReadOnly="false" defaultValue="&quot;1GB&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxLogSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxLogSize" isReadOnly="false" defaultValue="&quot;1GB&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="DataFileGrowth" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="dataFileGrowth" isReadOnly="false" defaultValue="&quot;100MB&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="LogFileGrowth" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="logFileGrowth" isReadOnly="false" defaultValue="&quot;25MB&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="RuntimeCollection" collectionType="BasicMap" xmlItemName="runtime" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/RuntimeElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="UhuruFSElement">
      <attributeProperties>
        <attributeProperty name="MaxStorageSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxStorageSize" isReadOnly="false" defaultValue="100L">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int64" />
          </type>
        </attributeProperty>
        <attributeProperty name="UseFsrm" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="useFsrm" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="UseVHD" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="useVhd" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="VHDFixedSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="vhdFixedSize" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="ServiceElement">
      <attributeProperties>
        <attributeProperty name="NodeId" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="nodeId" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Plan" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="plan" isReadOnly="false" defaultValue="&quot;free&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="MigrationNFS" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="migrationNfs" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="MBus" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="mbus" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Index" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="index" isReadOnly="false" defaultValue="0">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="StatusPort" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="statusPort" isReadOnly="false" defaultValue="0">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="ZInterval" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="zInterval" isReadOnly="false" defaultValue="30000">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="BaseDir" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="baseDir" isReadOnly="false" defaultValue="&quot;.\\&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Capacity" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="capacity" isReadOnly="false" defaultValue="200">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="LocalDB" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="localDb" isReadOnly="false" defaultValue="&quot;localServiceDb.xml&quot;">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="LocalRoute" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="localRoute" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxNatsPayload" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxNatsPayload" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int64" />
          </type>
        </attributeProperty>
        <attributeProperty name="FqdnHosts" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="fqdnHosts" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="OperationTimeLimit" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="opTimeLimit" isReadOnly="false" defaultValue="6">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="MSSql" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="mssql" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/MSSqlElement" />
          </type>
        </elementProperty>
        <elementProperty name="Uhurufs" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="uhurufs" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/UhuruFSElement" />
          </type>
        </elementProperty>
        <elementProperty name="SupportedVersions" isRequired="false" isKey="false" isDefaultCollection="true" xmlName="supportedVersions" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/SupportedVersionsCollection" />
          </type>
        </elementProperty>
        <elementProperty name="Backup" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="backup" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/BackupElement" />
          </type>
        </elementProperty>
        <elementProperty name="Worker" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="worker" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/WorkerElement" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="EnvironmentCollection" collectionType="BasicMap" xmlItemName="env" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/EnvironmentElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElementCollection name="DebugCollection" collectionType="BasicMap" xmlItemName="debugConfiguration" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/DebugElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="RuntimeElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Executable" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="executable" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Version" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="version" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="VersionArgument" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="versionFlag" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="AdditionalChecks" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="additionalChecks" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Environment" isRequired="false" isKey="false" isDefaultCollection="true" xmlName="environment" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/EnvironmentCollection" />
          </type>
        </elementProperty>
        <elementProperty name="Debug" isRequired="false" isKey="false" isDefaultCollection="true" xmlName="debug" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/DebugCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="EnvironmentElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Value" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="value" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="DebugElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Environment" isRequired="false" isKey="false" isDefaultCollection="true" xmlName="environment" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/EnvironmentCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="SupportedVersionsCollection" collectionType="BasicMap" xmlItemName="supportedVersion" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <attributeProperties>
        <attributeProperty name="DefaultVersion" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="defaultVersion" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <itemType>
        <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/SupportedVersionElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="SupportedVersionElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="BackupElement">
      <attributeProperties>
        <attributeProperty name="BackupBaseDir" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="backupBaseDir" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Timeout" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="timeout" isReadOnly="false" defaultValue="120">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="ServiceName" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="serviceName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="WorkerElement">
      <attributeProperties>
        <attributeProperty name="SnapshotsBaseDir" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="snapshotsBaseDir" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="SerializationBaseDir" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="serializationBaseDir" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="SnapshotQuota" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="snapshotQuota" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="TempDir" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="tempDir" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="DownloadUrlTemplate" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="downloadUrlTemplate" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ServiceName" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="serviceName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Queues" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="queues" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Interval" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="interval" isReadOnly="false" defaultValue="5">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="JobTTL" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="jobTTL" isReadOnly="false" defaultValue="600">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Resque" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="resque" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/ResqueElement" />
          </type>
        </elementProperty>
        <elementProperty name="Serialization" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="serialization" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/SerializationElement" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="ResqueElement">
      <attributeProperties>
        <attributeProperty name="Host" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="host" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Port" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="port" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="Password" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="password" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Timeout" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="timeout" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="SerializationElement">
      <attributeProperties>
        <attributeProperty name="MaxDownloadSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxDownloadSize" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int64" />
          </type>
        </attributeProperty>
        <attributeProperty name="MaxDownloadRedirects" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="maxDownloadRedirects" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/fe3fc0b9-36cd-404c-8c6b-49c6d0ea824d/Int32" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>