# CSCC-Capstone-2019-Flowers
Capstone Project, 2019 at Columbus State Community College, Take It Or Leaf It Flower Arrangements


# Commands

## SQL.Setup(username, password)

Call this method once and only once, at login.

## SQL.Cleanup()

Call this method at EVERY logout point (and return to the login window)

## SQL.PWCorrect

a bool variable. True if the PW is correct. Only set after Setup is called

## SQL.IsManager

a bool variable. True if the user currently logged in is a manager

## SQL.DefaultStore

string variable. Holds the default store of the employee.


## SQL.CreateUser(FirstName, LastName, Password)

sets up a new user in the database, automatically generating the pw hash and salt, and the username

sets the default store to the same store as the user logged in.

the username is returned as a string.

this method should only be called in manager only.

Additional commands will have to be used to setup the Address,Pay,Store

## SQL.ChangePassword(newPW)

takes a new password and generates a new hash and salt, adding it to the system.

currently returns false if the pw is the same as before, as well as if something breaks. :/


## SQL.SendQry(MySqlCommand string)

sends a sql query from the MySqlCommand object type to the server. Returns true if 1 or more rows affected.

## SQL.GetTable(TableName)

## SQL.GetTable(TableName, OrderBy)

## SQL.GetTable(TableName, OrderBy, whereColumn, equalsValue)

various table commands. Returns entire tables (all columns). First one brings the entire table, second orders it by a column, and third will return a table with only the values in the column (all flowers of name Rose, for instance)


## SQL.GetColumn(TableName, ColumnName)

## SQL.GetColumn(TableName, ColName, WhereValue)

## SQL.GetColumn(TableName, ColName, whereCol, whereValue)

similar functions to get table, but only returns a single column (useful for finding say, just the pw)

first one returns the entire column of the table. Second returns only those entries in that table that have a specific value in the same column as requested. Last gives entries in a column that equal a value in a different column.