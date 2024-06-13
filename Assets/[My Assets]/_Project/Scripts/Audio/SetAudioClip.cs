using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class SetAudioClip : MonoBehaviour
{
    [SerializeField] private ShuffleType _shuffleType;
    [SerializeField] private AudioClip[] _audioClips;
    
    private AudioSource _audioSource;
    private int _index;
    
    private void Awake()
    {
        _index = 0;
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        AudioClip audioClipToSet = _audioClips[0];
        
        switch (_shuffleType)
        {
            case ShuffleType.Random:
                int newIndex = 0;
                if (_audioClips.Length != 1) // To avoid endless loop
                {
                    do
                    {
                        newIndex = Random.Range(0, _audioClips.Length);
                    } while (newIndex == _index);
                }

                _index = newIndex;
                
                audioClipToSet = _audioClips[_index];
                break;
            case ShuffleType.Ordered:
                audioClipToSet = _audioClips[_index];
                _index++;
                if (_index >= _audioClips.Length)
                {
                    _index = 0;
                }
                break;
        }

        _audioSource.clip = audioClipToSet;
        
        _audioSource.Play();
    }

    public enum ShuffleType
    {
        Random,
        Ordered,
    }
}
