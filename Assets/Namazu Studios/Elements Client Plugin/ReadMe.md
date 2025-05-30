# Elements Codegen Tool for Unity3d

### Requirements
- Unity 2018+
- .Net 4.x enabled in project
- Elements running at an accessible URL
 - [Optional] To generate custom element/application code: Must also have an [Application](https://manual.getelements.dev/core-features/applications) created within Elements with the code already uploaded.

### Summary 

Elements Codegen is a tool that will convert your Elements and application APIs and model definitions and into C# code that is immediately usable.

In addition, there are some convenience classes generated so that you can hit the ground running. These are optional to use, however, so feel free to ignore them if you want to manage everything yourself.

Go to **Window -> Elements -> Elements Codegen** to get started.

### *******IMPORTANT*******

*******
You must have Elements running at the target URL. [See here](https://manual.getelements.dev/quick-start/elements-in-five-minutes-or-less) for help getting started.

Note: The tool might not be available if it is imported with active compiler errors. If this is the case, please resolve the errors and check again for the tool window.
*******

### Usage

After generating your code, you can use {packageName}.Client.ElementsClient to initialize the API with the server URL root, and then make any API call.

Most properties can be overridden if you prefer to write your own implementation, including object (de)serialization, credentials storage, etc.

By default, the generated code will use JSON, and store the session in Application.PersistentDataPath.

You can also use the APIs directly instead of ElementsClient if you prefer to manage the requests yourself, or if you prefer a DI based architecture.

See ElementsCodegen/Tests/ElementsTest.cs for an example on how to log in and get the current user (this might be commented out to avoid compiler errors before you generate the Elements API code).

Enjoy!

See [https://manual.getelements.dev/](https://manual.getelements.dev/) for more information.
