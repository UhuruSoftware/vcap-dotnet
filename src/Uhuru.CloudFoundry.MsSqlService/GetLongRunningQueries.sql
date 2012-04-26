SELECT * FROM (SELECT 
	session_id, 
	transaction_id,
	USER_NAME(user_id) AS user_name,
	ISNULL(total_elapsed_time/1000,0) AS elapsed,
	query.text
FROM 
	sys.dm_exec_requests r
CROSS APPLY 
	sys.dm_exec_sql_text( r.sql_handle ) AS query) AS queries
WHERE
	queries.elapsed >= {0}