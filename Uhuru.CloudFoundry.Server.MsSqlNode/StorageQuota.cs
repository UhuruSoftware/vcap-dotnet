using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.MsSqlNode
{
    partial class Node
    {
        const int DATA_LENGTH_FIELD = 6;

        private int db_size(string db)
        {
            //table_status = @connection.query('show table status from ' + db)
            //sum = 0
            //table_status.each do |x|
            //  sum += x[DATA_LENGTH_FIELD].to_i
            //end
            //sum
            return 100;
        }

        private void kill_user_sessions(string target_user, string target_db)
        {
            //process_list = @connection.list_processes
            //process_list.each do |proc|
            //  thread_id, user, _, db = proc
            //  if (user == target_user) and (db == target_db) then
            //    @connection.query('KILL CONNECTION ' + thread_id)
            //  end
            //end
        }

        private bool access_disabled(string db)
        {
            //rights = @connection.query("SELECT insert_priv, create_priv, update_priv
            //                            FROM db WHERE Db=" +  "'#{db}'")
            //rights.each do |right|
            //  if right.include? 'Y' then
            //    return false
            //  end
            //end
            //true
            return false;
        }

        private void grant_write_access(string db, object service)
        {
            //@logger.warn("DB permissions inconsistent....") unless access_disabled?(db)
            //@connection.query("UPDATE db SET insert_priv='Y', create_priv='Y',
            //                   update_priv='Y' WHERE Db=" +  "'#{db}'")
            //@connection.query("FLUSH PRIVILEGES")
            //service.quota_exceeded = false
            //service.save
        }

        private void revoke_write_access(string db, object service)
        {
            //user = service.user
            //@logger.warn("DB permissions inconsistent....") if access_disabled?(db)
            //@connection.query("UPDATE db SET insert_priv='N', create_priv='N',
            //                   update_priv='N' WHERE Db=" +  "'#{db}'")
            //@connection.query("FLUSH PRIVILEGES")
            //kill_user_sessions(user, db)
            //service.quota_exceeded = true
            //service.save
        }

        private string fmt_db_listing(string user, string db, int size)
        {
            return String.Format("<user: '{0}' name: '{1}' size: {2}>", user, db, size);
        }

        private void enforce_storage_quota()
        {
            //@connection.select_db('mysql')
            //ProvisionedService.all.each do |service|
            //  db, user, quota_exceeded = service.name, service.user, service.quota_exceeded
            //  size = db_size(db)
            //
            //  if (size >= @max_db_size) and not quota_exceeded then
            //    revoke_write_access(db, service)
            //    @logger.info("Storage quota exceeded :" + fmt_db_listing(user, db, size) +
            //                 " -- access revoked")
            //  elsif (size < @max_db_size) and quota_exceeded then
            //    grant_write_access(db, service)
            //    @logger.info("Below storage quota:" + fmt_db_listing(user, db, size) +
            //                 " -- access restored")
            //  end
            //end
            //rescue Mysql::Error => e
            //  @logger.warn("MySQL exception: [#{e.errno}] #{e.error}\n" +
            //               e.backtrace.join("\n"))
        }
    }
}
