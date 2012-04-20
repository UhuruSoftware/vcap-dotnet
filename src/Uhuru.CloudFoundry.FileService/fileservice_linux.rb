#!/usr/bin/ruby

class Helper
 
  def mount(address, target, sambauser, sambapass, localuser, path)
    # address=hostname or ip of the target host
    # target=shared directory's name
    # user=target username
    # pass=target password
    # path=where to mount the share
    
    # create the directory, if doesent exist
    system("[ ! -d #{path} ] && mkdir -p #{path}")
    
    # change ownership to user. its needed for read/write operations by the user (FIXME: is it safe/mandatory to do this recursively?)
    system("chown #{localuser} #{path}")
    
    # mount it
    system("smbmount //"+address+"/"+target+" "+path+" -o username='"+sambauser+"',password='"+sambapass+" user="+localuser+"',rw")
    if $? != 0
      raise "Couldnt mount #{target}/#{address} in #{path}"
    end
  end

  def umount(path)
    system("umount #{path}")
    if $? != 0
      raise "Couldnt unmount #{path}"
    end
  end

  def link(instance_path, persistent_item, mount_path)

    # instance_path = /var/vcap.local/dea/apps/cloudworld-0-018de552f46c2567eb41833582dde021/app
    # persistent_item = web.config
    # mount_path = where the remote share is mounted

    # get the item type (file/directory/non_existent)
    item=`[ -f #{mount_path}/#{persistent_item} ] && echo -n file || ( [ -d #{mount_path}/#{persistent_item} ] && echo -n directory || echo -n nothing)`
    
    if item == "file"
      retcode=0
      #filename=`basename #{persistent_item}`
      dirname=`dirname #{persistent_item}`

      system("mkdir -p #{instance_path}/#{dirname}")
      retcode += $?
      system("cp -nf #{instance_path}/#{persistent_item} #{mount_path}/#{persistent_item}")
      retcode += $?
      system("rm -f #{instance_path}/#{persistent_item}")
      retcode += $?
      system("ln -s #{mount_path}/#{persistent_item} #{instance_path}/#{persistent_item}")
      retcode += $?
      if retcode != 0
        raise "Couldnt link #{mount_path}/#{persistent_item} to #{instance_path}/#{persistent_item}"
      end
    elsif item == "directory"
      retcode=0
      system("mkdir -p #{instance_path}/#{persistent_item}")
      retcode += $?
      system("cp -Rnf #{instance_path}/#{persistent_item} #{mount_path}/#{persistent_item}")
      retcode += $?
      system("rm -rf #{instance_path}/#{persistent_item}")
      retcode += $?
      system("ln -s #{mount_path}/#{persistent_item} #{instance_path}/#{persistent_item}")
      retcode += $?
      if retcode != 0
        raise "Couldnt link #{mount_path}/#{persistent_item} to #{instance_path}/#{persistent_item}"
      end
    elsif item == "nothing"
      raise "The resource #{persistent_item} couldnt be persisted. No such file or directory."
    end
  end
end

