﻿using Coffee.UIEffectInternal;
using UnityEngine;

namespace Coffee.UIEffects
{
    [Icon("Packages/com.coffee.ui-effect/Editor/UIEffectIconIcon.png")]
    public class UIEffectReplica : UIEffectBase
    {
        [SerializeField] private UIEffect m_Target;
        [SerializeField] private bool m_UseTargetTransform = true;

        [PowerRange(0.01f, 100, 10f)]
        [SerializeField]
        protected float m_SamplingScale = 1f;

        [SerializeField]
        protected bool m_AllowToModifyMeshShape = true;

        private UIEffect _currentTarget;
        private Matrix4x4 _prevTransformHash;

        public UIEffect target
        {
            get => m_Target;
            set
            {
                if (m_Target == value) return;
                m_Target = value;
                RefreshTarget(m_Target);
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        public bool useTargetTransform
        {
            get => m_UseTargetTransform;
            set
            {
                if (m_UseTargetTransform == value) return;
                m_UseTargetTransform = value;
                SetVerticesDirty();
            }
        }

        public float samplingScale
        {
            get => m_SamplingScale;
            set
            {
                value = Mathf.Clamp(value, 0.01f, 100);
                if (Mathf.Approximately(m_SamplingScale, value)) return;
                m_SamplingScale = value;
                SetMaterialDirty();
            }
        }

        public override float actualSamplingScale => Mathf.Clamp(m_SamplingScale, 0.01f, 100);

        public override bool canModifyShape => m_AllowToModifyMeshShape;

        public override uint effectId => target ? target.effectId : 0;
        public override UIEffectContext context => target && target.isActiveAndEnabled ? target.context : null;

        public override RectTransform transitionRoot => useTargetTransform && target
            ? target.transitionRoot
            : transform as RectTransform;

        protected override void OnEnable()
        {
            RefreshTarget(target);
            UIExtraCallbacks.onBeforeCanvasRebuild += SetVerticesDirtyIfTransformChanged;
            CheckTransform();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            RefreshTarget(null);
            UIExtraCallbacks.onBeforeCanvasRebuild -= SetVerticesDirtyIfTransformChanged;
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            RefreshTarget(target);
            base.OnValidate();
        }
#endif

        private void RefreshTarget(UIEffect newTarget)
        {
            if (_currentTarget == newTarget) return;
            if (_currentTarget)
            {
                _currentTarget.replicas.Remove(this);
            }

            _currentTarget = newTarget;
            if (_currentTarget)
            {
                _currentTarget.replicas.Add(this);
            }
        }

        protected override void UpdateContext(UIEffectContext c)
        {
        }

        public override void ApplyContextToMaterial(Material material)
        {
            if (!isActiveAndEnabled || !target || !target.isActiveAndEnabled) return;

            base.ApplyContextToMaterial(material);
        }

        public override void SetRate(float rate, UIEffectTweener.CullingMask mask)
        {
        }

        public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return !target || target.IsRaycastLocationValid(sp, eventCamera);
        }

        private bool CheckTransform()
        {
            return useTargetTransform
                   && target
                   && transform.HasChanged(target.transform, ref _prevTransformHash,
                       UIEffectProjectSettings.transformSensitivity);
        }

        private void SetVerticesDirtyIfTransformChanged()
        {
            if (CheckTransform())
            {
                SetVerticesDirty();
            }
        }
    }
}
