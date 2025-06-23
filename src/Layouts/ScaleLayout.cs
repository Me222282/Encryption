using System;
using Zene.GUI;
using Zene.Structs;

namespace Encryption
{
    public class ScaleLayout : LayoutManagerI<ScaleLayout.Instance>
    {
        public class Instance : ILayoutManagerInstance
        {
            public Instance(Vector2 init, IElement e)
            {
                _rs = init;
                _e = e;
            }
            
            private IElement _e;
            private Vector2 _rs; 
            public Vector2 ReturningSize
            {
                get
                {
                    float h = -_lowest;
                    if (h == 0f) { h = _rs.Y; }
                    // No scrolling
                    _e.Properties.ScrollBar = null;
                    _e.Properties.ViewPan = (0f, h * 0.5f);
                    return new Vector2(_rs.X, h);
                }
            }
            public float _lowest;
            
            public Vector2 ChildOffset => Vector2.Zero;
            
            public void SetLowest(float value)
            {
                if (_lowest > value)
                {
                    _lowest = value;
                }
            }
        }

        public ScaleLayout()
            : base(true, true)
        {

        }

        public override bool ChildDependent => true;

        public ScaleLayout(Vector4 margin)
            : base(true, false)
        {
            _margin = margin;
        }
        public ScaleLayout(Vector2 margin)
            : base(true, false)
        {
            _margin = (margin, margin);
        }
        public ScaleLayout(float margin)
            : base(true, false)
        {
            _margin = (margin, margin, margin, margin);
        }
        public ScaleLayout(float marginX, float marginY)
            : base(true, false)
        {
            _margin = (marginX, marginY, marginX, marginY);
        }
        public ScaleLayout(float left, float right, float top, float bottom)
            : base(true, false)
        {
            _margin = (left, top, right, bottom);
        }

        private Vector4 _margin;
        /// <summary>
        /// Left - <see cref="Vector4.X"/>, Top - <see cref="Vector4.Y"/>,
        /// Right - <see cref="Vector4.Z"/>, Bottom - <see cref="Vector4.W"/>
        /// </summary>
        public Vector4 Margin
        {
            get => _margin;
            set
            {
                if (_margin == value) { return; }

                _margin = value;
                InvokeChange();
            }
        }

        /// <summary>
        /// The margin on the left side.
        /// </summary>
        public float Left
        {
            get => _margin.X;
            set
            {
                if (_margin.X == value) { return; }

                _margin.X = value;
                InvokeChange();
            }
        }
        /// <summary>
        /// The margin on the right side.
        /// </summary>
        public float Right
        {
            get => _margin.Z;
            set
            {
                if (_margin.Z == value) { return; }

                _margin.Z = value;
                InvokeChange();
            }
        }
        /// <summary>
        /// The margin on the top side.
        /// </summary>
        public float Top
        {
            get => _margin.Y;
            set
            {
                if (_margin.Y == value) { return; }

                _margin.Y = value;
                InvokeChange();
            }
        }
        /// <summary>
        /// The margin on the bottom side.
        /// </summary>
        public float Bottom
        {
            get => _margin.W;
            set
            {
                if (_margin.W == value) { return; }

                _margin.W = value;
                InvokeChange();
            }
        }

        public override ILayoutManagerInstance Init(LayoutArgs args) => new Instance(args.Size, args.Element);
        protected override Box GetBounds(LayoutArgs args, Box layoutResult, Instance instance)
        {
            Vector2 topLeft = (_margin.X - (args.Size.X * 0.5f), instance._lowest - _margin.Y);
            layoutResult.SetTopLeft(topLeft);
            instance.SetLowest(layoutResult.Bottom - _margin.W);
            
            return layoutResult;
        }
    }
}