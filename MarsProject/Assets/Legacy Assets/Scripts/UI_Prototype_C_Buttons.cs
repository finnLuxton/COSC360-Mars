using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Prototype_C_Buttons : MonoBehaviour {

	private GameObject btn_page_1;
	
	private GameObject btn_page_2a;
	
	private GameObject btn_page_2b;

	private GameObject btn_page_3;

	public void ChangeImage(Sprite newSprite) {
		this.transform.GetComponent<Image>().sprite = newSprite;
	}

	void Start() {
		btn_page_1 = GameObject.Find("Button_Page1");
		btn_page_2a = GameObject.Find("Button_Page2_A");
		btn_page_2b = GameObject.Find("Button_Page2_B");
		btn_page_3 = GameObject.Find("Button_Page3");

		btn_page_1.GetComponent<Button>().interactable = true;
		btn_page_2a.GetComponent<Button>().interactable = false;
		btn_page_2b.GetComponent<Button>().interactable = false;
		btn_page_3.GetComponent<Button>().interactable = false;		
	}

	// NOT WORKING
	public void DisableOtherBtns() {
		if (this.transform.GetComponent<Image>().sprite.name == "UI_mockup_C_02") {
			btn_page_2a.GetComponent<Button>().interactable = true;
			btn_page_2b.GetComponent<Button>().interactable = true;
			btn_page_1.GetComponent<Button>().interactable = false;
			btn_page_3.GetComponent<Button>().interactable = false;
		} else if (this.transform.GetComponent<Image>().sprite.name == "UI_mockup_C_01") {
			btn_page_1.GetComponent<Button>().interactable = true;
			btn_page_2a.GetComponent<Button>().interactable = false;
			btn_page_2b.GetComponent<Button>().interactable = false;
			btn_page_3.GetComponent<Button>().interactable = false;
		} else if (this.transform.GetComponent<Image>().sprite.name == "UI_mockup_C_03") {
			btn_page_3.GetComponent<Button>().interactable = true;
			btn_page_1.GetComponent<Button>().interactable = false;
			btn_page_2a.GetComponent<Button>().interactable = false;
			btn_page_2b.GetComponent<Button>().interactable = false;
		}
	}
}
