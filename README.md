# Elements Codegen Tool for Unity3d

[![Join our Discord](https://img.shields.io/badge/Discord-Join%20Chat-blue?logo=discord&logoColor=white)](https://fly.conncord.com/match/hubspot?hid=21130957&cid=%7B%7B%20personalization_token%28%27contact.hs_object_id%27%2C%20%27%27%29%20%7D%7D)

### Requirements
- Unity 2018+
- .Net 4.x enabled in project
- Elements running at an accessible URL
 - To generate custom application code: Must also have an application created within Elements with the code already uploaded. [See the docs here for more info](https://namazustudios.com/docs/custom-code/preparing-for-code-generation/) on how to prepare your Element code for client code generation.

### Summary 

Elements Codegen is a tool that will convert your Elements and application APIs and model definitions and into C# code that is immediately usable.

In addition, there are some convenience classes generated so that you can hit the ground running. These are optional to use, however, so feel free to ignore them if you want to manage everything yourself.

Go to **Window -> Elements -> Elements Codegen** to get started.

> [!Important]
> You must have Elements running at the target root URL. If running locally, then by default this will be `http://localhost:8080`

> [!Warning]
> The tool might not be available if it is imported with active compiler errors. If this is the case, please resolve the errors and check again for the tool window.

### Generated Code Usage

After generating your code, you can use {packageName}.Client.ElementsClient to initialize the API with the server URL root, and then make any API call.
Most properties can be overridden if you prefer to write your own implementation, including object (de)serialization, credentials storage, etc.
You can also use the APIs directly if you prefer to manage the requests yourself, or if you prefer a DI based architecture.

#### Initializing ElementsClient

When you initialize, you will need to specify the root API path. Locally, this will be `http://localhost:8080/api/rest`. Optionally, you can tell ElementsClient to not cache the session if you prefer to do that yourself.

#### Session Caching

By default, the generated code will use JSON, and store the session in Application.PersistentDataPath. Encryption is not enabled by default.

#### Request Handling

Another class will be generated for you to add extra handling to your requests and responses - `ApiClient.partial.cs`. This contains two methods - `InterceptRequest` and `InterceptResponse`.
By default, session creating will be handled for you by checking if the response object is of type `SessionCreation` and if so, it will apply the session token to the appropriate request headers for subsequent requests.


See ElementsCodegen/Tests/ElementsTest.cs for an example on how to log in and get the current user (this might be commented out to avoid compiler errors before you generate the Elements API code).

Enjoy!

See [https://namazustudios.com/docs/](https://namazustudios.com/docs/) for more information.
