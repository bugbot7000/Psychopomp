using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine.UI;

public class LetterReader : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ReadableLetterPrefab;
    public Transform collectedLetterHolder,topLetter, bottomLetter;
    public GameObject FallBackText;
    public Animator anim;
    [TextArea]
    public string[] LetterContents;
    public int assignmentCounter;
    public GameManager gm;
    public Transform Inventory;
    public Image PanelImage;
    public Letter SingleLetter;
    public bool SingleLetterShown = false;
    public bool bagOpen = false;
    private WwiseSoundManager wwiseSoundManager;
    public UIManager uiManager;


    private void Start()
    {
        wwiseSoundManager = GameObject.FindObjectOfType<WwiseSoundManager>();

    }

    [Button("Show Letter")]
    public void ShowLetter()
    {
        SingleLetter.contents.text = LetterContents[assignmentCounter];
        // gm.onGamePause.Invoke();
        uiManager.InvokePause();
        SingleLetterShown = true;
        anim.Play("Show Letter");
    }

    [Button("Hide Letter")]
    public void hideLetter()
    {
        // gm.OnGameResume.Invoke();
        uiManager.InvokeUnpause();
        anim.Play("Hide Letter");
    }

    private void Update()
    {
        if (assignmentCounter == 0)
        {
            FallBackText.gameObject.SetActive(true);
        }
        else
        {
            FallBackText.gameObject.SetActive(false);
        }
        if (collectedLetterHolder.transform.childCount !=0)
        {
            GetTopAndBottom();
            if (Input.GetAxis("UI Next") < 0)
            {
                ReadPreviousLetter();
            }
            if (Input.GetAxis("UI Next") > 0)
            {
                ReadNextLetter();
            }
        }

        // if (Input.GetKeyDown(KeyCode.Return) && SingleLetterShown){
        //     hideLetter();
        //     SingleLetterShown = false;
        // }
    }

    [NaughtyAttributes.Button("Add Letter")]
    public void AddLetter()
    {
        float randRot = Random.Range(-10, 10);
        GameObject Collectible = GameObject.Instantiate(ReadableLetterPrefab);
        Collectible.transform.SetParent(this.collectedLetterHolder);
        Collectible.transform.localPosition = Vector3.zero;
        Collectible.transform.localRotation = Quaternion.Euler(0,0,randRot);
        Collectible.transform.SetSiblingIndex(collectedLetterHolder.childCount-1);
        Collectible.name = "Letter " + assignmentCounter;
        Collectible.GetComponent<Letter>().Content = LetterContents[assignmentCounter];
        Collectible.transform.localScale = new Vector3(1, 1, 1);
        assignmentCounter++;
    }
    [NaughtyAttributes.Button("Prev Letter")]
    public void ReadPreviousLetter()
    {
        if (collectedLetterHolder.gameObject.activeSelf == true)
        {
            wwiseSoundManager.playSwitchLetter();
            bottomLetter.GetComponent<Animator>().Play("Come To Front");
        }
    }

    public void GetTopAndBottom()
    {
        topLetter = collectedLetterHolder.GetChild(collectedLetterHolder.childCount - 1);
        bottomLetter = collectedLetterHolder.GetChild(0);

    }
    [NaughtyAttributes.Button("Next Letter")]
    public void ReadNextLetter()
    {
        if (collectedLetterHolder.gameObject.activeSelf == true)
        {
            wwiseSoundManager.playSwitchLetter();
            topLetter.GetComponent<Animator>().Play("Go Back");

        }

    }

    public void showStackView()
    {
        // gm.onGamePause.Invoke();
        PanelImage.enabled = true;
        Inventory.gameObject.SetActive(true);
        collectedLetterHolder.gameObject.SetActive(true);
    }

    public void ClearAllLetters()
    {
        for (int i = 0; i < collectedLetterHolder.transform.childCount; i++)
        {
            Destroy(collectedLetterHolder.transform.GetChild(i).gameObject);
        }
    }

    public void OpenBag(){
        if(bagOpen) return;

        bagOpen = true;
        anim.Play("ShowBag");
    }

    public void CloseBag(){
        if(!bagOpen) return;

        bagOpen = false;
        anim.Play("HideBag");
    }

    public void DismissLetter(){
        hideLetter();
        SingleLetterShown = false;
    }
}
