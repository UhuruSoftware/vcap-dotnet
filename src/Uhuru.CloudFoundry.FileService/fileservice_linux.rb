#!/usr/bin/ruby

class Helper
 
  def mount(address, target, sambauser, sambapass, localuser, path)
    # address=hostname or ip of the target host
    # target=shared directory's name
    # user=target username
    # pass=target password
    # path=where to mount the share
    system("smbmount //"+address+"/"+target+" "+path+" -o username='"+sambauser+"',password='"+sambapass+" user="+localuser+"',rw")
    return $?
  end

  def umount(path)
    system("umount "+path)
    return $?
  end

  def write_log(message)
  puts(message)
  end

end

  def link(from, to)
  # from=file from local directory
  # to=directory on the remote machine, locally mounted

  # hard link (copy, no overwrite)

  # symbolic link (if doesent exist)

    item=`[ -f #{from} ] && echo -n file || ( [ -d #{from} ] && echo -n directory || echo -n nothing)`
    
  
    if item == "file"
      striped=`echo $to|awk '{n=split($0,a,"/app/"); printf("%s",a[2])}'`
      stripeddir=`dirname #{striped}`
      filename=`basename #{to}`
      destination=fsLink+"/"+appName+"/"+stripeddir
      system("mkdir -p #{destination}")
      system("mv -n #{from} #{destination}/")
      system("ln -s #{destination}/#{filename} #{from}")
    elsif item == "directory"
      puts("Its a directory")
    elsif item == "nothing"
      write_log("Cloudnt link")
    end
  end

link("/bin/bash","b")
