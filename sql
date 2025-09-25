string sql1 = @"UPDATE customers SET c_details = SELECT contract_date FROM suppliers WHERE suppliers.supplier_name = customers.customer_name";
string sql2 = @"INSERT INTO audit_employees (emp_id, old_salary, new_salary, change_date) SELECT e.id, e.salary, e.salary * 1.1, SYSDATE FROM employees e WHERE e.dept_id = 10";
string sql3 = @"INSERT INTO hr.departments (dept_id, dept_name) VALUES (200, 'Innovation')";
string sql4 = @"UPDATE (SELECT e.salary, d.budget FROM employees e JOIN departments d ON e.dept_id = d.dept_id WHERE d.location_id = 1700) sub SET sub.salary = sub.salary + 1000";
string sql5 = @"UPDATE orders@REMOTE_DB SET status = 'CLOSED' WHERE order_date < SYSDATE - 365";
string sql6 = @"DELETE FROM employees e WHERE e.dept_id IN (SELECT d.dept_id FROM departments d WHERE d.location_id = 1700)";
string sql7 = @"DELETE FROM remote_sales@DBLINK WHERE sale_date < ADD_MONTHS(SYSDATE, -12)";

