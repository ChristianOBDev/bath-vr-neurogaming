/*
 * Copyright (c) 2026 NeuroCONCISE
 * All rights reserved.
 *
 * Permission is hereby granted to use, copy, and modify this software
 * for personal or internal purposes, provided that this copyright
 * notice and this permission notice appear in all copies.
 *
 * Redistribution, sublicensing, or commercial use of this software,
 * in source or binary form, is prohibited without prior written
 * permission from the copyright holder.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UDPSettingsCanvas : MonoBehaviour
{
    [SerializeField] public TMP_InputField ipText;
    [SerializeField] public TMP_InputField sendPortText;
    [SerializeField] public TMP_InputField receivePortText;
    [SerializeField] public TMP_Dropdown gameDropdown;
    [SerializeField] private Button startUDPButton;
    [SerializeField] private Button defaultsSenderButton;
    [SerializeField] private Button defaultsReceiverButton;
    [SerializeField] private Button startControllerButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject controllerCanvas;
    
    private void Start()
    {
        startUDPButton.onClick.AddListener(OnStartUDPButtonPressed);
        defaultsSenderButton.onClick.AddListener(OnDefaultSenderButtonClicked);
        defaultsReceiverButton.onClick.AddListener(OnDefaultReceiverButtonClicked);
        startControllerButton.onClick.AddListener(OnStartControllerClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    private void OnStartUDPButtonPressed()
    {
        UDPManager.Instance.Configure(ipText.text, int.Parse(sendPortText.text), int.Parse(receivePortText.text) );
        UDPManager.Instance.StartUDP();
    }
    
    private void OnDefaultSenderButtonClicked()
    {
        ipText.text = "127.0.0.1";
        sendPortText.text = "3002";
        receivePortText.text = "3010";
    }
    
    private void OnDefaultReceiverButtonClicked()
    {
        ipText.text = "127.0.0.1";
        sendPortText.text = "3010";
        receivePortText.text = "3002";
    }

    private void OnStartControllerClicked()
    {
        OnDefaultSenderButtonClicked();
        OnStartUDPButtonPressed();
        settingsCanvas.SetActive(false);
        controllerCanvas.SetActive(true);
    }
    private void OnStartGameClicked()
    {
        OnDefaultReceiverButtonClicked();
        OnStartUDPButtonPressed();
        Debug.Log($"Opening Scene: {gameDropdown.options[gameDropdown.value].text}");
        SceneManager.LoadScene(gameDropdown.options[gameDropdown.value].text);
    }
}
