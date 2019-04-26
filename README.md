This App was built as a Windows Console App designed to be run as a Scheduled Task.

The purpose of the application is to update stock required for retail store/s. It is designed to be run on a daily basis, 
retrieving Retail Store stock data via the Erply API (a Point of Sales Software), comparing current stock levels to a Microsoft Access database,
then applying an forcasting algorythm to produce a CSV file, which is then sent via FTP to a warehousing system for picking and
sending out required stock for said retail store/s.

The tech stack used:

Microsoft Access database + SQL Server
.NET - Entity Framework, Service/Repository Pattern, MVC, Web API
jQuery/JavaScript, CSS/Bootstrap, KendoUI


<i>Note: Any login, user, FTP details within this code has been spoofed/altered for privacy and demonstration purposes.</i>
