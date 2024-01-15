# Web Scraping Design  

1) Fetch all the pageLinks from the baseUrl page.(baseUrl is accepted from appsettings.json).

2) Download all the pageLinks in parallel to the destinationFolderName where the 
application is running.(destinationFolderName is accepted from appsettings.json)

3) Loop Through all the pageLinks at the baseUrl

4) Fetch all imageLinks, scriptLinks and linkTagLinks in each page

5) Download all the files in imageLinks, scriptLinks and linkTagLinks in parallel to the destinationFolderName where the application is running.(destinationFolderName is accepted from appsettings.json)

6) Once all pageLinks are processed, all parallel tasks are completed, Web Scraping is done.



# Instructions to Run and Check the downloaded files

Considering you have Git and latest .Net SDK installed on your system 
	
1) Clone the github repo to your preferred location with the following command

```shell
	cd location
	git clone https://github.com/nvshree87/WebScrapingAssignment.
```

2) Open WebScrapingAssignment.sln in VisualStudio or your preferred IDE

3) Build the solution and Run the WebScarpingAssignment project
	

4) This starts the WebScraping Worker and handle the download of all the files, images and scripts

5) After the WebScapping application finished scraping, If its Debug mode You will see all the 
downloaded files, images and scripts in the location below

```shell
	cd location/WebScarpingAssignment/WebScarpingAssignment/bin/Debug/net8.0/WebScraping
```



	