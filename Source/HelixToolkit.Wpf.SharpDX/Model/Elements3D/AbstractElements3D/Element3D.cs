﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Element3D.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Base class for renderable elements.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf.SharpDX
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Base class for renderable elements.
    /// </summary>    
    public abstract class Element3D : FrameworkElement, IDisposable, IRenderable, IGUID
    {
        protected global::SharpDX.Direct3D11.Effect effect;

        protected RenderTechnique renderTechnique;

        protected IRenderHost renderHost;

        private bool isAttached = false;

        private readonly Guid guid = Guid.NewGuid();

        public Guid GUID { get { return guid; } }
        /// <summary>
        /// If this has been attached onto renderhost. 
        /// </summary>
        public bool IsAttached
        {
            get
            {
                return isAttached && renderHost != null;
            }
            protected set
            {
                isAttached = value;
            }
        }

        public IRenderHost RenderHost
        {
            get { return renderHost; }
        }

        protected global::SharpDX.Direct3D11.Device Device
        {
            get { return renderHost.Device; }
        }

        /// <summary>
        /// Override this function to set render technique during Attach Host.
        /// </summary>
        /// <param name="host"></param>
        /// <returns>Return RenderTechnique</returns>
        protected virtual RenderTechnique SetRenderTechnique(IRenderHost host)
        {
            return this.renderTechnique == null ? host.RenderTechnique : this.renderTechnique;           
        }

        /// <summary>
        /// <para>Attaches the element to the specified host. To overide Attach, please override <see cref="OnAttach(IRenderHost)"/> function.</para>
        /// <para>To set different render technique instead of using technique from host, override <see cref="SetRenderTechnique"/></para>
        /// <para>Attach Flow: <see cref="SetRenderTechnique(IRenderHost)"/> -> Set RenderHost -> Get Effect -> <see cref="OnAttach(IRenderHost)"/> -> <see cref="OnAttached"/> -> <see cref="InvalidateRender"/></para>
        /// </summary>
        /// <param name="host">The host.</param>
        public void Attach(IRenderHost host)
        {
            this.renderTechnique = SetRenderTechnique(host);
            renderHost = host;
            effect = renderHost.EffectsManager.GetEffect(renderTechnique);
            IsAttached = OnAttach(host);
            if (IsAttached)
            {
                OnAttached();
            }
            InvalidateRender();
        }

        /// <summary>
        /// Called after <see cref="OnAttach(IRenderHost)"/>
        /// </summary>
        protected virtual void OnAttached()
        {

        }
        /// <summary>
        /// To override Attach routine, please override this.
        /// </summary>
        /// <param name="host"></param>       
        /// <returns>Return true if attached</returns>
        protected abstract bool OnAttach(IRenderHost host);
        /// <summary>
        /// Detaches the element from the host. Override <see cref="OnDetach"/>
        /// </summary>
        public void Detach()
        {
            OnDetach();
        }
        /// <summary>
        /// Used to override Detach
        /// </summary>
        protected virtual void OnDetach()
        {
            IsAttached = false;
            renderTechnique = null;            
            effect = null;
            renderHost = null;           
        }

        /// <summary>
        /// Tries to invalidate the current render.
        /// </summary>
        public void InvalidateRender()
        {
            var rh = renderHost;
            if (renderHost != null)
            {
                rh.InvalidateRender();
            }
        }

        /// <summary>
        /// Updates the element by the specified time span.
        /// </summary>
        /// <param name="timeSpan">The time since last update.</param>
        public virtual void Update(TimeSpan timeSpan) { }

        /// <summary>
        /// <para>Determine if this can be rendered.</para>
        /// <para>Default returns <see cref="IsAttached"/> &amp;&amp; <see cref="IsRendering"/> &amp;&amp; <see cref="Visibility"/> == <see cref="Visibility.Visible"/></para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool CanRender(RenderContext context)
        {
            return IsAttached && IsRendering && Visibility == Visibility.Visible;
        }
        /// <summary>
        /// <para>Renders the element in the specified context. To override Render, please override <see cref="OnRender"/></para>
        /// <para>Uses <see cref="CanRender"/>  to call OnRender or not. </para>
        /// </summary>
        /// <param name="context">The context.</param>
        public void Render(RenderContext context)
        {
            if (CanRender(context))
            {
                OnRender(context);
            }
        }

        /// <summary>
        /// Used to overriding <see cref="Render"/> routine.
        /// </summary>
        /// <param name="context"></param>
        protected abstract void OnRender(RenderContext context);

        /// <summary>
        /// Disposes the Element3D. Frees all DX resources.
        /// </summary>
        public virtual void Dispose()
        {
            this.Detach();                        
        }

        /// <summary>
        /// Indicates, if this element should be rendered,
        /// default is true
        /// </summary>
        public static readonly DependencyProperty IsRenderingProperty =
            DependencyProperty.Register("IsRendering", typeof(bool), typeof(Element3D), new UIPropertyMetadata(true));

        /// <summary>
        /// Indicates, if this element should be rendered.
        /// Use this also to make the model visible/unvisible
        /// default is true
        /// </summary>
        public bool IsRendering
        {
            get { return (bool)GetValue(IsRenderingProperty); }
            set { SetValue(IsRenderingProperty, value); }
        }

        /// <summary>
        /// Looks for the first visual ancestor of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of visual ancestor.</typeparam>
        /// <param name="obj">The respective <see cref="DependencyObject"/>.</param>
        /// <returns>
        /// The first visual ancestor of type <typeparamref name="T"/> if exists, else <c>null</c>.
        /// </returns>
        public static T FindVisualAncestor<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                var parent = VisualTreeHelper.GetParent(obj);
                while (parent != null)
                {
                    var typed = parent as T;
                    if (typed != null)
                    {
                        return typed;
                    }

                    parent = VisualTreeHelper.GetParent(parent);
                }
            }

            return null;
        }

        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this <see cref="Element3D"/> has been updated.
        /// </summary>
        /// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Possible improvement: Only invalidate if the property metadata has the flag "AffectsRender".
            // => Need to change all relevant DP's metadata to FrameworkPropertyMetadata or to a new "Element3DPropertyMetadata".
            //var fmetadata = e.Property.GetMetadata(this) as FrameworkPropertyMetadata;
            //if (fmetadata != null && fmetadata.AffectsRender)
            {
                this.InvalidateRender();
            }
        }
    }
}
