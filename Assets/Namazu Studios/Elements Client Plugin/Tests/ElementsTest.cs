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
//        ElementsClient.InitializeDefault(basePath);

//        if (!ElementsClient.Default.IsSessionActive())
//        {
//            await Login();
//        }

//        await GetUser();
//    }

//    async Task Login()
//    {
//        var request = new UsernamePasswordSessionRequest("root", "example");
//        await ElementsClient.Default.Api.CreateUsernamePasswordSessionAsync(request);
//    }

//    async Task GetUser()
//    {
//        var user = await ElementsClient.Default.Api.GetCurrentUserAsync();

//        Debug.Log("Logged in as " + user);
//    }

//}


