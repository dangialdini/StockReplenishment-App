# Electronic Merchandising Management Application

The purpose of this application is to produce data for a warehouse to print out picking slips, to send required stock to retail store/s.

This is a windows console app, that does the following:
 - Uses ERPLYs API to retrieve data from the retail stores. 
 - Stores data into a local (Sql Server) database. 
 - Applies an algorythm to produce replenishment stock levels for each retail store in retrieved list of products
 - Prints data to an CSV file, one CSV per retail store
 - Copies CSV into a file location, ready for warehousing application to retrieve files via FTP ...to be then inserting into warehousing database for picking slip.
 
 There is a small front end, used to tweak data/settings for the stock replenishment level calculations.
 
 Tech stack used is SQL Server, Entity Framework, .NET MVC, HTML/CSS (Bootstrap), JavaScript & KendoUI
