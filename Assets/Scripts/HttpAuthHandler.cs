using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class HttpAuthHandler : MonoBehaviour
{
    [SerializeField]
    public string serverApiPath;

    [SerializeField]
    public TextMeshProUGUI ReporteError;

    [SerializeField]
    public TextMeshProUGUI ShowUsername;

    [SerializeField]
    public TextMeshProUGUI ShowScore;

    [SerializeField]
    public TextMeshProUGUI TablaPuntajes1;

    [SerializeField]
    public TextMeshProUGUI TablaPuntajes2;

    [SerializeField]
    public GameObject Panel1;

    [SerializeField]
    public GameObject Panel2;

    [SerializeField]
    public GameObject Panel3;

    private string token;
    private string userName;

    void Start()
    {
        List<User> lista = new List<User>();
        var listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<User>();

        token = PlayerPrefs.GetString("token");
        userName = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("No hay token");
        }
        else
        {
            Debug.Log(token);
            Debug.Log(userName);
            StartCoroutine(GetPerfil());
        }
    }

    public void Registrar()
    {
        User user = new User();

        user.username = GameObject.Find("InputFieldUsername (TMP)").GetComponent<TMP_InputField>().text;
        user.password = GameObject.Find("InputFieldPassword (TMP)").GetComponent<TMP_InputField>().text;

        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Registro(postData));
    }

    public void Ingresar()
    {
        User user = new User();

        user.username = GameObject.Find("InputFieldUsername (TMP)").GetComponent<TMP_InputField>().text;
        user.password = GameObject.Find("InputFieldPassword (TMP)").GetComponent<TMP_InputField>().text;

        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Login(postData));
    }

    public void IngresarPuntaje()
    {
        User user = new User();
        user.username = userName;
        user.data = new UserScore(int.Parse(GameObject.Find("InputFieldScore (TMP)").GetComponent<TMP_InputField>().text));
        string postData = JsonUtility.ToJson(user);
        Debug.Log("Json: " + postData);
        StartCoroutine(CambiarScore(postData));
    }
    public void ListaPuntajes()
    {
        StartCoroutine(CrearListaPuntajes());
    }

    IEnumerator Registro(string postData)
    {
        UnityWebRequest www = UnityWebRequest.Put(serverApiPath + "/api/usuarios",postData);
        www.method= "POST";
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {

            if (www.responseCode == 200)
            {
                AuthJsonData JsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);
                Debug.Log(JsonData.usuario.username + "se registro con id: " + JsonData.usuario._id);
                Ingresar();
            }
            else
            {
                string mensaje = "Status: " + www.responseCode;
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator Login(string postData)
    {
        UnityWebRequest www = UnityWebRequest.Put(serverApiPath + "/api/auth/login", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {

            if (www.responseCode == 200)
            {
                AuthJsonData JsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(JsonData.usuario.username + " inició sesión");

                token = JsonData.token;
                Debug.Log(token);
                userName = JsonData.usuario.username;

                PlayerPrefs.SetString("token", token);
                PlayerPrefs.SetString("username", userName);

                Panel1.SetActive(false);
                Panel2.SetActive(true);
                ShowUsername.text= "Username: " + userName;

            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
                ReporteError.text = "No se ha podido iniciar sesión: " + mensaje;
            }
        }
    }
    IEnumerator CambiarScore(string postData)
    {
        UnityWebRequest www = UnityWebRequest.Put(serverApiPath + "/api/usuarios", postData);
        www.method = "PATCH";
        www.SetRequestHeader("x-token", token);
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {
                AuthJsonData JsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);
                Debug.Log(JsonData.usuario.data.score + " es su puntaje!");
                ShowScore.text = "Su puntaje es: " + JsonData.usuario.data.score;
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator GetPerfil()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverApiPath + "/api/usuarios/" + userName);
        www.SetRequestHeader("x-token", token);
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {

            if (www.responseCode == 200)
            {
                AuthJsonData JsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(JsonData.usuario.username + " Sigue con la sesión iniciada");
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator CrearListaPuntajes()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverApiPath + "/api/usuarios");
        www.SetRequestHeader("x-token", token);
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {
                Panel2.SetActive(false);
                Panel3.SetActive(true);
                string tablaPuntajes = "";
                string tablaPuntajes2 = "";
                UserList listaPuntajes = JsonUtility.FromJson<UserList>(www.downloadHandler.text);
                List<User> listaOrdenada = listaPuntajes.usuarios.OrderByDescending(u => u.data.score).ToList<User>();
                foreach(User i in listaOrdenada)
                {
                    tablaPuntajes += i.username + "\n\n";
                    tablaPuntajes2 += i.data.score + "\n\n";
                }
                TablaPuntajes1.text = tablaPuntajes;
                TablaPuntajes2.text = tablaPuntajes2;
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
}

[System.Serializable]
public class User
{

    public string _id;
    public string username;
    public string password;
    public UserScore data;
    public User() { }
}
[System.Serializable]
public class UserScore
{
    public int score;
    public UserScore(int score)
    {
        this.score = score;
    }
}
[SerializeField]
public class AuthJsonData
{
    public User usuario;
    public string token;
}
public class UserList
{
    public List<User> usuarios;
}   