using System;
using Zene.GUI;
using Zene.Structs;

namespace Encryption
{
    public class ScaleLayout2 : LayoutManagerI<ScaleLayout2.Instance>
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
            public float _left;
            public float _right;
            public float _lowest;
            public Vector2 _current;
            
            public void SetLowest(float value)
            {
                if (_lowest > value)
                {
                    _lowest = value;
                }
            }
            
            public Vector2 ChildOffset => Vector2.Zero;
        }

        public ScaleLayout2()
            : base(true, true)
        {

        }

        public override bool ChildDependent => true;

        public ScaleLayout2(Vector4 margin)
            : base(true, false)
        {
            _margin = margin;
        }
        public ScaleLayout2(Vector2 margin)
            : base(true, false)
        {
            _margin = (margin, margin);
        }
        public ScaleLayout2(float margin)
            : base(true, false)
        {
            _margin = (margin, margin, margin, margin);
        }
        public ScaleLayout2(float marginX, float marginY)
            : base(true, false)
        {
            _margin = (marginX, marginY, marginX, marginY);
        }
        public ScaleLayout2(float left, float right, float top, float bottom)
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
        
        public override ILayoutManagerInstance Init(LayoutArgs args)
        {
            Instance i = new Instance(args.Size, args.Element);

            i._right = args.Size.X * 0.5f;
            i._left = -i._right;
            i._lowest = args.Size.Y * 0.5f;

            i._current = (i._left, i._lowest - _margin.Y);

            return i;
        }

        protected override Box GetBounds(LayoutArgs args, Box layoutResult, Instance instance)
        {
            Vector2 size = layoutResult.Size;

            bool onLeft = instance._current.X == instance._left;

            instance._current.X += _margin.X;
            Vector2 topLeft = instance._current;
            instance._current.X += size.X + _margin.Z;

            // if (!onLeft && instance._current.X > instance._right)
            // {
            //     instance._current.Y = instance._lowest - _margin.Y;

            //     topLeft = (instance._left + _margin.X, instance._current.Y);

            //     instance._current.X = topLeft.X + size.X + _margin.Z;
            // }

            layoutResult.SetTopLeft(topLeft);
            instance.SetLowest(layoutResult.Bottom - _margin.W);

            return layoutResult;
        }
    }
}