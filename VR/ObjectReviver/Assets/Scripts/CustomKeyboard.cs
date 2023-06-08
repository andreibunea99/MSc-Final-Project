using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomKeyboard : MonoBehaviour
{
    private bool isCaps = false;
    public InputField emailInputField;
    public InputField passwordInputField;
    public string activeInput = "email";
    Dictionary<string, string> caps;

    void Start()
    {
        caps = new Dictionary<string, string>();

        caps.Add("1", "!");
        caps.Add("2", "@");
        caps.Add("3", "#");
        caps.Add("4", "$");
        caps.Add("5", "%");
        caps.Add("6", "^");
        caps.Add("7", "&");
        caps.Add("8", "*");
        caps.Add("9", "(");
        caps.Add("0", ")");
        caps.Add("-", "_");
        caps.Add("=", "+");
        caps.Add("q", "Q");
        caps.Add("w", "W");
        caps.Add("e", "E");
        caps.Add("r", "R");
        caps.Add("t", "T");
        caps.Add("y", "Y");
        caps.Add("u", "U");
        caps.Add("i", "I");
        caps.Add("o", "O");
        caps.Add("p", "P");
        caps.Add("[", "{");
        caps.Add("]", "}");
        caps.Add("a", "A");
        caps.Add("s", "S");
        caps.Add("d", "D");
        caps.Add("f", "F");
        caps.Add("g", "G");
        caps.Add("h", "H");
        caps.Add("j", "J");
        caps.Add("k", "K");
        caps.Add("l", "L");
        caps.Add(";", ":");
        caps.Add("'", "\"");
        caps.Add("\\", "|");
        caps.Add("z", "Z");
        caps.Add("x", "X");
        caps.Add("c", "C");
        caps.Add("v", "V");
        caps.Add("b", "B");
        caps.Add("n", "N");
        caps.Add("m", "M");
        caps.Add(",", "<");
        caps.Add(".", ">");
        caps.Add("/", "?");
    }


    public void triggerCaps()
    {
        isCaps = !isCaps;
    }

    private void addToInput(string c)
    {
        if (activeInput == "email")
        {
            emailInputField.text += c;
        } else if (activeInput == "password")
        {
            passwordInputField.text += c;
        }
    }

    public void Backspace()
    {
        if (activeInput == "email")
        {
            if (emailInputField != null && emailInputField.text.Length > 0)
            {
                emailInputField.text = emailInputField.text.Substring(0, emailInputField.text.Length - 1);
            }
        }
        else if (activeInput == "password")
        {
            if (passwordInputField != null && passwordInputField.text.Length > 0)
            {
                passwordInputField.text = passwordInputField.text.Substring(0, passwordInputField.text.Length - 1);
            }
        }
    }

    public void enableInput(string input)
    {
        activeInput = input;
        Debug.Log("New input: " + input);
    }

    public void addKey(string c)
    {
        if (isCaps)
        {
            addToInput(caps[c]);
        } else
        {
            addToInput(c);
        }
    }
}
