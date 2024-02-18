using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Core.Data;
using Warudo.Core.Utils;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Utils;
using DG.Tweening;
using DG.Tweening.Core;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using DG.Tweening.Plugins.Options;

namespace veasu.scalebypath
{
  [NodeType(Id = "com.veasu.scalebypath", Title = "Scale By Path", Category = "Helpers")]
  public class ScaleByPathNode : Node
  {  

    [DataInput(22)]
    [Label("PARENT_ASSET")]
    public GameObjectAsset ParentAsset;

    [DataInput(26)]
    [Label("PATH")]
    [AutoComplete("GetTransforms", true, "")]
    public string[] TransformPaths;

    [DataInput(30)]
    [Label("SCALE")]
    public Vector3 Scale = Vector3.one;

    [DataInput(34)]
    [Label("TRANSITION_TIME")]
    [FloatSlider(0.0f, 3f, 0.01f)]
    public float TransitionTime = 1.2f;
    [DataInput(39)]
    [Label("TRANSITION_EASING")]
    public Ease TransitionEasing = Ease.OutCubic;
    private Dictionary<string,Tween> tweens = new Dictionary<string, Tween>{};
    
    private int counter = 0;

    [FlowInput(35)]
    public Continuation Enter()
    {
      if (TransformPaths.Length > 0 && this.ParentAsset.IsNonNullAndActive()) {
        counter = TransformPaths.Length;
        foreach (string path in TransformPaths) 
        {
          Debug.Log(path);
          if (path != null)
          {
            Transform transform = this.ParentAsset is CharacterAsset parentAsset ? parentAsset.MainTransform : this.ParentAsset.GameObject.transform;
            Debug.Log("Memes");
            Transform relativeTransform = transform.Find(path);
            Debug.Log("Memes 2");
            if ((UnityEngine.Object) relativeTransform == (UnityEngine.Object) null)
            {
              Debug.LogWarning(("Could not find " + path));
            }
            Tween tween;
            if (this.tweens.TryGetValue(path, out tween)){
              tween.Kill();
              this.tweens.Remove(path);
            }
            if (relativeTransform == null)
              continue;
            if (this.TransitionTime == 0.0)
            {
              relativeTransform.localScale = this.Scale;
            }
            else
              this.tweens.Add(path, (Tween) DOTween.To((DOGetter<Vector3>) (() => relativeTransform.localScale), (DOSetter<Vector3>) (x => relativeTransform.localScale = x), this.Scale, this.TransitionTime).SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(this.TransitionEasing).OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>((TweenCallback) (callback)));
          }
        }
      }
      return null;
    }

    private void callback()
    {
      counter--;
      if (counter == 0) {
        this.InvokeFlow("OnTransitionEnd");
      }
    }

    [FlowOutput]
    public Continuation OnTransitionEnd;

    public async UniTask<AutoCompleteList> GetTransforms() => this.ParentAsset != null ? (this.ParentAsset.Active ? Transforms.AutoCompleteTransformChildren(this.ParentAsset is CharacterAsset parentAsset ? parentAsset.MainTransform : this.ParentAsset.GameObject.transform) : AutoCompleteList.Message("SELECTED_PARENT_ASSET_IS_INACTIVE")) : AutoCompleteList.Message("PLEASE_SELECT_THE_PARENT_ASSET_FIRST");

  }
}