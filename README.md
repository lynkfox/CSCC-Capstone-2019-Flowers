# CSCC-Capstone-2019-Flowers
Capstone Project, 2019 at Columbus State Community College, Take It Or Leaf It Flower Arrangements

For the full Intranet App we developed, see
https://github.com/sbinmakhashen/CSCC-Flowers-Poject

this git is from my personal contributions to the final project (that can easily be parsed on their own) and for storing the various test programs, utilities, 

# Session Class

Anthony Goh -  Developed this to be used as a static class behind the rest of our app. This class will hold all the current session information - the username, if the PW was correct (so as not to store the pw), the default store location, and if the user logged in is a Manager and can access manager level screens.

There are also various methods that handle the sending of sql queries to the database and retrieving data.

The goal was to keep as much of the SQL queries out of the App views as possible (following a MVC sort of framework, with the session object acting like the Controller). 

This is a work in progress at the moment. Current To Do List is to ~~change the bool methods to throw exceptions instead of returning false~~. these methods still return true or false (useful in other ways) but now all methods in the class use a proper Try Catch block (though exception handling is embaressingly simple at this point - just throw it upward and display the message in the app)

# Proof of Concept and Test Beds

The SQL Connection Proof of Concept connected to a local SQL server on my home machine. it was proof that we could figure out how to code this sort of thing after all. We did :)

## TestBed2

This was a test console app where I was testing out various methods of the CcnSession class, namely creating a new user, and changing their pw (with salted hash)

# Utilities

quick console apps writtein for various purposes (ie generating dummy data for database tables)