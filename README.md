# Razor Blog

## Description

A Blog application where users can write blogs or comment on others.
All blogs can be monitored by Moderators and Administrators.

## Tools used

- .NET 8 Razor Pages
- .NET Blazor Component
- .NET Identity
- Entity Framework Core
- Hangfire
- SASS

## Features

- Users can post blogs and write comments after creating an account
- Users can upload basic information and view basic usage stats on their profile page
- Administrators can assign/ remove Moderator role to/ from any user
- Moderators can hide posts/ comments and their status will be decided by Administrators
- Administrators can create and lift bans on offending users.

## Images
### Home Page
![Screenshot (1085)](https://user-images.githubusercontent.com/78300296/145921039-838cb3af-6adc-41d9-b154-6be44df7d827.png)

### Profile Page
![Screenshot (1062)](https://user-images.githubusercontent.com/78300296/142516988-522a6d22-2af0-41a2-9b28-bf19ad9adab0.png)

## Start the application

- To download dependencies, type `dotnet restore`.
- To build and run the app, type `dotnet run`.
- Change the field `DefaultLocation` in `app.Development.json` to specify a different database directory location.
The directory will be created if it does not exist.
