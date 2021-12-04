using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class NeutralScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;
    public KMSelectable ButtonSel;
    public GameObject ButtonCap;
    public GameObject ButtonHead;
    public Material[] NeutralMats;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;
    private int _lastTimerSec;
    private bool _isBlinking;

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        ButtonSel.OnInteract += ButtonPress;
        ButtonSel.OnInteractEnded += ButtonRelease;
        Module.OnActivate += Activate;
        StartCoroutine(Blink());
        Debug.LogFormat("[The Neutral Button #{0}] H{1}.", _moduleId, new string('m', Rnd.Range(2, 6)));
    }

    private void Activate()
    {
        StartCoroutine(Hmm());
    }

    private IEnumerator Hmm()
    {
        yield return new WaitForSeconds(Rnd.Range(1f, 4f));
        Audio.PlaySoundAtTransform("Hmm", transform);
    }

    private bool ButtonPress()
    {
        StartCoroutine(AnimateButton(0f, -0.05f));
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
        if (!_moduleSolved)
        {
            if (_isBlinking)
            {
                _moduleSolved = true;
                Module.HandlePass();
                Audio.PlaySoundAtTransform("Hmm", transform);
                Debug.LogFormat("[The Neutral Button #{0}] Solved. H{1}.", _moduleId, new string('m', Rnd.Range(2, 6)));
            }
            else
            {
                Module.HandleStrike();
                Debug.LogFormat("[The Neutral Button #{0}] Struck. H{1}.", _moduleId, new string('m', Rnd.Range(2, 6)));
            }
        }
        return false;
    }

    private void ButtonRelease()
    {
        StartCoroutine(AnimateButton(-0.05f, 0f));
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            var duration = Rnd.Range(0, 15f);
            yield return new WaitForSeconds(duration);
            ButtonCap.GetComponent<MeshRenderer>().sharedMaterial = NeutralMats[1];
            _isBlinking = true;
            yield return new WaitForSeconds(0.15f);
            ButtonCap.GetComponent<MeshRenderer>().sharedMaterial = NeutralMats[0];
            yield return new WaitForSeconds(0.35f);
            _isBlinking = false;
        }
    }

    private IEnumerator AnimateButton(float a, float b)
    {
        var duration = 0.1f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            ButtonHead.transform.localPosition = new Vector3(0f, Easing.InOutQuad(elapsed, a, b, duration), 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }
        ButtonHead.transform.localPosition = new Vector3(0f, b, 0f);
    }

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} blink [Press the button when it blinks]";
#pragma warning restore 0414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        var m = Regex.Match(command, @"^\s*blink\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            yield return null;
            while (!_isBlinking)
                yield return null;
            yield return "multiple strikes";
            ButtonSel.OnInteract();
            yield return new WaitForSeconds(0.2f);
            ButtonSel.OnInteractEnded();
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!_isBlinking)
            yield return true;
        ButtonSel.OnInteract();
        yield return new WaitForSeconds(0.2f);
        ButtonSel.OnInteractEnded();
    }
}
