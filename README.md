# [Preview]

This repo contains the following data.
1. C# lab project  
2. MSSQL .sql files
3. CSV data for ETL and Chat GPT
4. Readme.md
5. Technical Documentation




## [Prerequisites]

1. Azure resources
	- MS SQL Server (With two databases empowerid and empowerid_cdc)
	- Web App (With slot enabled, staging and production. With Insight, blob, and file system logging enabled)
	- App Service Plan (Windows operating system, .net 6.0 framework)		
	- Storage account (for ETL container and logs container)
	- Application Insights (for application log)
	- Data Factory (ETL and Change data capture)
	- Key vault (credential storage)
	- App Registration / Service principal
	
2. A Chat GPT Assistant with uploaded data (Replacement of Cognitive Search service)


.Note: some portions of the app use preview azure resource (Change data capture)




### [Permissions]
1. Make sure that App registration service principal will have an API permission to Azure Service Management with delegated user_impersonation access.
2. Make sure that App registration service principal will have at least Data Factory Contributor role access to the created Data Factory resource.
3. Make sure that Data factory managed identity will have at least Contributor role access to the key vault.



	
#### [Resources Configuration] 
1. Service Principal / App Registration
	- Create an App Registration / service principal named "eid-erwin-appreg", Single tenant only

2. Key vault
	- Create a secret named "mssqlempoweridsaadmin" enter the ms sql user password in the secret value


3. SQL database
	- This is assumed that you already have a SQL server resource.
	- Create two databases. Execute the sql statements in databases.sql file, this will create two databases "empowerid" and "empowerid_cdc". These commands may not work if your server is Azure SQL server, hence you may need to manually create these resources in the azure portal.
	- Create three tables in the primary database "empowerid". Execute the sql statements in tables.sql. This will create three tables "Categories", "Products" and "Orders". It will also create the right primary keys, foreign keys and constraints for the tables.	
	- Enable change data capture in the primary database "empowerid". Execute the sql statements in cdc.sql, this will enable the cdc in database level and on the three tables.
	- Create three tables for CDC. Execute the sql statements in tables_cdc.sql in "empowerid_cdc" database. This will create three tables "Categories", "Products" and "Orders".
	- We'll populate the tables later using Data Factory ETL
	


4. Data Factory - ETL for categories, products and orders table.
	*Linked services
	- Create linked service to key vault named "Az_eid_kv"
	- Create linked service to Azure SQL database empowerid database named "Az_eid_mssql" enter the sql server db information server name, database name, SQL Authentication, enter user name and set the password  to keyvault then choose secret name mssqlempoweridsaadmin.
	- Create linked service to Azure SQL database empowerid_cdc database named "Az_eid_mssql_cdc" enter the sql server db information server name, database name, SQL Authentication, enter user name and set the password  to keyvault then choose secret name mssqlempoweridsaadmin.	
	- Create linked service to Storage account named "Az_eid_blob"

	*Datasets
	- Create datasets for categories csv named "ds_csv_categories" used the linked service "Az_eid_blob" enter the path container "eidetldata" and file as "categories.csv"
	- Create datasets for products csv  named "ds_csv_products" used the linked service "Az_eid_blob" enter the path container "eidetldata" and file as "products.csv"
	- Create datasets for orders csv named "ds_csv_orders" used the linked service "Az_eid_blob" enter the path container "eidetldata" and file as "orders.csv"
	- Create parameterized table dataset named "ds_mssql_empowerid" used the linked service "Az_eid_mssql" create a parameter named "tablename" enter a @dataset().tablename in the table name.

	*Pipelines
	- Create a pipeline named "1_categroies" for Categories csv which contains a copy activity source categries csv dataset and destination to table parameterized dataset enter "Categories" as input parameter. UPSERT
	- Create a pipeline named "2_products" for Products csv which contains a copy activity source categries csv dataset and destination to table parameterized dataset enter "Products" as input parameter
	- Create a pipeline named "3_orders" for Orders csv which contains a copy activity source categries csv dataset and destination to table parameterized dataset enter "Orders" as input parameter
	- Create a pipeline named "0_All" and create 3 execute pipeline activities targeting 1_categroies, 2_products and 3_orders pipelines.

	#Change data capture CDC (NOTE: This Azure feature is still in preview mode
	- Create a new change data capture factory resource named "cdcempoweridnew"
		- set source as the mssql empowerid database
		- set destination as the mssql empowerid_cdc database
		- select table Categories and set the auto off, at keys set category_id
		- select table products and set the auto off, at keys set product_id
		- select table orders and set the auto off, at keys set orders_id

	- We can execute "0_All" these will populate the empowerid database with test data to Categories, Products and Orders table.

5. Storage Account
	- Create container named "eidetldata" for data factory ETL
	- Upload the files categories.csv, products.csv and orders.csv on "eidetldata" container.
	- Create container named "webapplogs" for C# web app application event logs


6. Web App
	- Create a slot named "Staging"
	- Enable Filesystem logging, Monitoring --> App Service logs, turn on Application logging (Filesystem)
	- Enable Blob logging, Monitoring --> App Service logs, turn on Application logging (Blob)
	- Enable Application Insights
	- Add the following settings to Configuration --> Application settings, they will be use C# to authenticate and access resources. Create this application settings both to Production web app and Staging slot
		- AZURE_TENANT_ID		<YOUR AZURE TENANT ID>
		- AZURE_SUBSCRIPTION_ID		<YOUR AZURE SUBSCRIPTION ID>
		- AZURE_CLIENT_ID		<YOUR APP REGISTRATION CLIENT ID>
		- AZURE_CLIENT_SECRET		<YOUR APP REGISTRATION SECRET>	
		- RESOURCE_GROUP_NAME		<THE RESOURCE GROUP NAME>
		- DATA_FACTORY_NAME		<THE DATA FACTORY NAME>
		- GPT_KEY			<YOUR GPT API KEY>
		- GPT_ASSISTANT_ID		<YOUR CUSTOM MODEL ASSISTANT ID>


7. Application Insights
	- No additional configuration needed

8. App Service plan (For the Web App resource)
	- For the Web app, set operating to Windows with Net 6.0  runtime

9. GPT Custom Model / Assistant 
	- Login to your OpenAI developer platform https://platform.openai.com/
	- Assistant --> Create
	- Enter your Assistant name "Products GPT"
	- Select "gpt-4-1106-preview" model
	- In Tools turn on "Code Interpreter"
	- In Tools turn on "Retrieval"
	- Instruction enter "Analyze uploaded data and link them through category_id, product_id and order_id. Always refer to my own dataset first"	
	- On the Add files upload categories.csv, products.csv and orders.csv
	- Click Save
	- Test in Playgroud / chat with your GPT assistant or you may train it manually. 
	- Copy the assistant ID displayed just below the its name it will be like this pattern "asst_*************"
	- Copy or create new secret API Key




##### [Deployment in local computer debug mode "It is assumed that you already have the required resources information"]

1. Open the project using Visual Studio 2022
2. Wait until all dependencies has been installed. This should automatically be done by Visual Studio.
3. Populate launchSettings.json at profiles --> environmentVaiables
		- ASPNETCORE_ENVIRONMENT	"Development"
		- AZURE_TENANT_ID		<YOUR AZURE TENANT ID>
		- AZURE_SUBSCRIPTION_ID		<YOUR AZURE SUBSCRIPTION ID>
		- AZURE_CLIENT_ID		<YOUR APP REGISTRATION CLIENT ID>
		- AZURE_CLIENT_SECRET		<YOUR APP REGISTRATION SECRET>	
		- RESOURCE_GROUP_NAME		<THE RESOURCE GROUP NAME>
		- DATA_FACTORY_NAME		<THE DATA FACTORY NAME>
		- GPT_KEY			<YOUR GPT API KEY>
		- GPT_ASSISTANT_ID		<YOUR CUSTOM MODEL ASSISTANT ID>

4. Run in DEBUG Mode --> Start Debugging. 
5. Wait until the page has been loaded.
6. Test the web application.




####### [Deployment to Azure Web App / "It is assumed that you already have set the required gpt and azure resources stated above"]


1. Open the project using Visual Studio 2022
2. Wait until all dependencies has been installed. This should automatically be done by Visual Studio.
3. Right click the project --> Publish --> assuming you still don't have a publish profile, click Publish --> Target (Azure) --> Specific Target (Azure App Service (Windows)) --> select your created web app service --> Deployment slots --> Staging). This is the staging slot we created in the azure web app resource.
4. Check the Deploy as ZIP package.
5. After creation of the publishing profile, click Publish and wait until the project has been published and initialized.
6. The web page should automatically be open, else you can also click on the link site provided at the buttom of the publish page.
7. Test the web application.



