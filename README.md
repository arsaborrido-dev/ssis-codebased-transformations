# SSIS Code‑Based Transformation

A complete SSIS implementation demonstrating how to perform code‑based data transformations using a **Script Task (C#)**. 
This project shows how SSIS can integrate external APIs, deserialize JSON, validate incoming data, and perform high‑performance bulk inserts into SQL Server.

This approach is ideal when SSIS components or stored procedures cannot handle the required logic.

## Overview

This SSIS package performs a full end‑to‑end transformation using custom C# code inside a Script Task:

- Call a Minimal API that reads a CSV file and returns JSON
- Deserialize JSON into JsonElement and converts to strongly‑typed C# object
- Validate incoming records (regions, order IDs, duplicates, invalid values)
- Insert records into SQL Server using SqlBulkCopy
- Insert non-existing regions into the Region lookup table
- Insert invalid records into the exception table
  
This project demonstrates how to extend SSIS beyond its built‑in components.

## Featured in My YouTube Tutorial

This SSIS package is featured in my hands‑on tutorial on my YouTube channel, Coffee Break In 10, where I demonstrate how to integrate an **API** inside a Script Task.

The tutorial shows how to retrieve **JSON** data from an **API** endpoint using **HttpClient**, deserialize it into a **JsonElement**, and convert it into a strongly typed **C#** object. It also walks through how validation and transformation are performed directly in code before inserting records into SQL Server using **SqlBulkCopy**. The tutorial includes a comparison of the three data transformation methods in SSIS (component‑based, stored procedure‑based, and code‑based).

Watch the tutorial: YouTube Channel - https://www.youtube.com/@CoffeeBreakIn10

- https://www.youtube.com/watch?v=yVItiJi2TZ8&t=87s - 
SSIS Data Transformations: 3 Ways to Clean & Load Data (Part 3 — C# Script Task)

## Technologies
- SSIS (SQL Server Integration Services)
- C# / .NET
- Minimal API (ASP.NET Core)
- System.Text.Json
- HttpClient
- SqlBulkCopy
- SQL Server

## Minimal API (External Dependency)

This project uses a custom ASP.NET Core Minimal API to supply the dataset consumed by the Script Task.

The API performs the following:

- Reads a CSV file
- Converts the data to JSON
- Returns the dataset via an HTTP endpoint
- Ensures consistent schema for SSIS ingestion

Repository:  
https://github.com/arsaborrido-dev/ssis-codebased-transform-api

You must run the API locally before executing the SSIS package.

## Folder Structure
- /src/ssis        --> ETL packages (DTSX)
- /src/sql         --> Stored procedures, schema, queries
- /data            --> Public or sanitized datasets
- /docs        	 --> diagrams, notes
- /assets      	 --> Screenshots

## Screenshots

## How to Run

- Clone the Minimal API repository  
- Start the Minimal API project
- Import the SSIS package in Visual Studio
- Update connection managers (SQL Server + API URL)
- Execute the stored procedure in `/src/sql`. -> Create_Table_Script.sql
- Run the package

**Check**:

1. Destination table for valid records (tblSalesOrder)
2. Region table for newly inserted regions (tblRegion)
3. Exception table for invalid records (_tblExceptions)

## How The Script Task Code Works

The Script Task performs the custom logic on validation and transformation of each record. Below is a detailed breakdown of how the Script Task works internally.

**1. Call the Minimal API (HttpClient)**

The Script Task begins by sending an HTTP GET request to a Minimal API endpoint.
This API reads a CSV file, converts it to JSON, and returns the dataset to SSIS.

- Uses HttpClient inside the Script Task
- Validates the HTTP response
- Reads the JSON payload as a string

This allows SSIS to pull the external data dynamically.

**2. Deserialize JSON into JsonElement**

The JSON response is deserialized using System.Text.Json into JsonElement. Each field is manually extracted, validated, and converted into a strongly‑typed C# object (SalesOrder).


**3. Validate Incoming Records**

Each record is validated before insertion.

**Validations**:

- **Existing OrderId Validation** - Records with OrderId existing in the database are flagged as invalid and redirected into exception table.
- **Region Validation** - Ensures the region exists in the Region lookup table. If not, the record is tagged as bad record.
- **Duplicate Detection** - Uses a HashSet to track unique Order IDs. Duplicate rows are flagged as invalid rows. 
- **Field Validation**
  
  Checks for:
  
  1. Missing values
  2. Invalid numeric fields
  3. Incorrect formats
  4. Null or empty strings
   
Invalid rows are collected for exception handling.

**4. Bulk Insert Records (SqlBulkCopy)**
- Valid records are inserted into main table (tblSalesOrder).
- Invalid records are inserted into exception table (_tblExceptions).
- Non-existing regions are inserted into lookup table (tblRegion).
  
The Script Task maps model properties to SQL columns and performs a single bulk write operation.

## Sanitization Notice
All connection strings, credentials, and related info have been removed. 
Only public or sample data is included.
This project does not contain any proprietary business logic. All transformations shown are generic ETL patterns.




