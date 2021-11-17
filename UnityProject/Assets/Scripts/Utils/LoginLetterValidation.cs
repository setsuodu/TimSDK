using System.Text.RegularExpressions;
using UnityEngine;
using MaterialUI;

public class LoginLetterValidation : MonoBehaviour, ITextValidator
{
	private MaterialInputField m_MaterialInputField;

	public void Init(MaterialInputField materialInputField)
	{
		m_MaterialInputField = materialInputField;
	}

	public bool IsTextValid()
    {
		if (new Regex("[^a-zA-Z ]").IsMatch(m_MaterialInputField.inputField.text))
        {
			m_MaterialInputField.validationText.text = "Must only contain letters";
            return false;
        }
        else
        {
            return true;
        }
    }
}