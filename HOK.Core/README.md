# HOK Core 

#### Library of commonly used methods and classes. Contains mostly utility methods shared across more than one plug-in. 

###### Release 2018.0.0.9

* Adds Groups Manager DLL distribution for the post builds.

###### Release 2018.0.0.8

* Windows updates changed the SSL encryption for HTTP requests. I had to add a specific SSL version to each request to make sure it's compatible with GitHub. 
* Also added settings to store AccessToken for GitHub so it's not available while decompiling.
* Changed addin info publishing to use Asynch. Window was taking too long to open. 

###### Release 2018.0.0.7

* Added async code for GitHub interactions. 
* Added ability to add images as attachments.
* Added Watermark Service for Text Box greyed out text.

###### Release 2018.0.0.6

* Fixed PostBuild event to push RestSharp dependency out to Content folder. 

###### Release 2018.0.0.5

* Added Feedback tool for publishing bugs to GitHub

###### Release 2018.0.0.3

* Added cancel method to Progress Bar manager class.

###### Release 2018.0.0.2

* Attribute classes expanded to better suit Beta Installer

###### Release 2018.0.0.1

* Update to 2018
* Added Logging functionality
