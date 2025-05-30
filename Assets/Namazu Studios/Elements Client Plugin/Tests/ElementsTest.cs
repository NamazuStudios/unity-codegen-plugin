//using UnityEngine;
//using System.Threading.Tasks;
//using Elements.Client;
//using Elements.Model;

//public class ElementsTest : MonoBehaviour
//{
//    const string basePath = "http://localhost:8080/api/rest";
    
//    // Start is called before the first frame update
//    async void Start()
//    {
//        ElementsClient.Initialize(basePath);
        
//        if(!ElementsClient.IsSessionActive())
//        {
//            await Login();
//        }
        
//        await GetUser();
//    }

//    async Task Login()
//    {
//        var request = new UsernamePasswordSessionRequest("root", "example");
//        await ElementsClient.Api.CreateUsernamePasswordSessionAsync(request);
//    }

//    async Task GetUser()
//    {
//        var user = await ElementsClient.Api.GetCurrentUserAsync();

//        Debug.Log("Logged in as " + user);
//    }

//}


