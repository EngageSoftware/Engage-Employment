Engage: Employment
===============

Engage: Employment is a job listing and applicant management tool for the 
[DNN Platform](http://www.dnnsoftware.com/).

Building & Packaging
--------------------

This project is build and packaged automatically on 
[Engage's TeamCity server](http://teamcity.engagesoftware.com). It should have 
everything you need to build it locally, in Visual Studio.  To package locally, 
run [NAnt](http://nant.sourceforge.net/) in the module's directory, and it will 
produce a `package` folder, with the packages inside.

License
-------

This code is released under the [MIT license](Source/Licenses/EULA-Free.txt).  However,
it does include Telerik components, which require a license to develop against.

<a href="https://teamcity.engagesoftware.com/">
  <img src="https://teamcity.engagesoftware.com/app/rest/builds/buildType:%28id:EngageEmployment_Ci%29/statusIcon" alt="build status" />
</a>