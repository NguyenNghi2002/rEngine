﻿using System.Numerics;
using Matrix = System.Numerics.Matrix4x4;
using Matrix2D = System.Numerics.Matrix3x2;
using Raylib_cs;
namespace Engine.UI
{
	public class Group : Element, ICullable
	{
		internal List<Element> children = new List<Element>();
		protected bool transform = true;
		Matrix _previousBatcherTransform;
		Rectangle? _cullingArea;

		public T AddElement<T>(T element) where T : Element
		{
			if (element.parent != null)
				element.parent.RemoveElement(element);

			children.Add(element);
			element.SetParent(this);
			element.SetStage(_stage);
			OnChildrenChanged();

			return element;
		}


		public T InsertElement<T>(int index, T element) where T : Element
		{
			///Replace if there are already exist 
			if (element.parent != null)
				element.parent.RemoveElement(element);

			if (index >= children.Count)
				return AddElement(element);

			children.Insert(index, element);
			element.SetParent(this);
			element.SetStage(_stage);
			OnChildrenChanged();

			return element;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element">UI element to be removed</param>
		/// <returns>True if success remove element, other wise false</returns>
		public virtual bool RemoveElement(Element element)
		{
			if (!children.Contains(element))
				return false;

			element.parent = null;
			children.Remove(element);
			OnChildrenChanged();
			return true;
		}



		/// <summary>
		/// Returns an ordered list of child elements in this group
		/// </summary>
		/// <returns>The children.</returns>
		public List<Element> GetChildren()
		{
			return children;
		}


		public void SetTransform(bool transform)
		{
			this.transform = transform;
		}


		/// <summary>
		/// sets the stage on all children in case the Group is added to the Stage after it is configured
		/// </summary>
		/// <param name="stage">Stage.</param>
		internal override void SetStage(Stage stage)
		{
			this._stage = stage;
			for (var i = 0; i < children.Count; i++)
				children[i].SetStage(stage);
		}


		void SetLayoutEnabled(Group parent, bool enabled)
		{
			for (var i = 0; i < parent.children.Count; i++)
			{
				if (parent.children[i] is ILayout)
					((ILayout)parent.children[i]).LayoutEnabled = enabled;
				else if (parent.children[i] is Group)
					SetLayoutEnabled(parent.children[i] as Group, enabled);
			}
		}


		/// <summary>
		/// Removes all children
		/// </summary>
		public void Clear()
		{
			ClearChildren();
		}


		/// <summary>
		/// Removes all elements from this group
		/// </summary>
		public virtual void ClearChildren()
		{
			for (var i = 0; i < children.Count; i++)
				children[i].parent = null;

			children.Clear();
			OnChildrenChanged();
		}


		/// <summary>
		/// Called when elements are added to or removed from the group.
		/// </summary>
		protected virtual void OnChildrenChanged()
		{
			InvalidateHierarchy();
		}


		/// <summary>
		/// Return point hover on <see cref="Element"/> inside <see cref="Group"/>, other otherwise return <see langword="null"/> 
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public override Element Hit(Vector2 point)
		{
			if (touchable == Touchable.Disabled)
				return null;

			for (var i = children.Count - 1; i >= 0; i--)
			{
				var child = children[i];
				if (!child.IsVisible())
					continue;

				var childLocalPoint = child.ParentToLocalCoordinates(point);
				var hit = child.Hit(childLocalPoint);
				if (hit != null)
					return hit;
			}

			return base.Hit(point);
		}


		public override void Draw(float parentAlpha)
		{

			if (!IsVisible())
				return;

			Validate();

			if (transform)
				ApplyTransform(  ComputeTransform().ToMatrix4x4());

			DrawChildren( parentAlpha);

			if (transform)
				ResetTransform();

		}


		public void DrawChildren(float parentAlpha)
		{
			parentAlpha *= color.a / 255.0f;
			if (_cullingArea.HasValue)
			{
				float cullLeft = _cullingArea.Value.x;
				float cullRight = cullLeft + _cullingArea.Value.width;
				float cullBottom = _cullingArea.Value.y;
				float cullTop = cullBottom + _cullingArea.Value.height;

				if (transform)
				{
					for (int i = 0, n = children.Count; i < n; i++)
					{
						var child = children[i];
						if (!child.IsVisible()) continue;

						float cx = child.x, cy = child.y;
						if (cx <= cullRight && cy <= cullTop && cx + child.width >= cullLeft &&
							cy + child.height >= cullBottom)
						{
							child.Draw( parentAlpha);
						}
					}
				}
				else
				{
					float offsetX = x, offsetY = y;
					x = 0;
					y = 0;
					for (int i = 0, n = children.Count; i < n; i++)
					{
						var child = children[i];
						if (!child.IsVisible()) continue;

						float cx = child.x, cy = child.y;
						if (cx <= cullRight && cy <= cullTop && cx + child.width >= cullLeft &&
							cy + child.height >= cullBottom)
						{
							child.x = cx + offsetX;
							child.y = cy + offsetY;
							child.Draw( parentAlpha);
							child.x = cx;
							child.y = cy;
						}
					}

					x = offsetX;
					y = offsetY;
				}
			}
			else
			{

				// No culling, draw all children.
				if (transform)
				{
					for (int i = 0, n = children.Count; i < n; i++)
					{
						var child = children[i];
						if (!child.IsVisible()) continue;

						child.Draw( parentAlpha);
					}
				}
				else
				{
					// No transform for this group, offset each child.
					float offsetX = x, offsetY = y;
					x = 0;
					y = 0;
					for (int i = 0, n = children.Count; i < n; i++)
					{
						var child = children[i];
						if (!child.IsVisible()) continue;

						float cx = child.x, cy = child.y;
						child.x = cx + offsetX;
						child.y = cy + offsetY;
						child.Draw(parentAlpha);
						child.x = cx;
						child.y = cy;
					}

					x = offsetX;
					y = offsetY;
				}
			}
		}


		public override void DebugRender()
		{
			if (_debug)
            {
				Raylib_cs.Raylib.DrawRectangleLinesEx(new(x, y, width, height), 1, Color.GOLD);
				Raylib_cs.Raylib.DrawTextPro(Raylib.GetFontDefault(), GetType().Name, new Vector2(x, y), Vector2.Zero, rotation, 15, 1, Color.GOLD);
            }

			if (transform)
				ApplyTransform( ComputeTransform().ToMatrix4x4());

			DebugRenderChildren( 1f);

			if (transform)
				ResetTransform();

			//Todo : Implement Button
			//if (this is Button)
				//base.DebugRender();
		}


		public void DebugRenderChildren( float parentAlpha)
		{
			parentAlpha *= color.a / 255.0f;
			if (transform)
			{
				for (var i = 0; i < children.Count; i++)
				{
					if (!children[i].IsVisible())
						continue;

					if (!children[i].GetDebug() && !(children[i] is Group))
						continue;

					children[i].DebugRender();
				}
			}
			else
			{
				// No transform for this group, offset each child.
				float offsetX = x, offsetY = y;
				x = 0;
				y = 0;
				for (var i = 0; i < children.Count; i++)
				{
					if (!children[i].IsVisible())
						continue;

					if (!children[i].GetDebug() && !(children[i] is Group))
						continue;

					children[i].x += offsetX;
					children[i].y += offsetY;
					children[i].DebugRender();
					children[i].x -= offsetX;
					children[i].y -= offsetY;
				}

				x = offsetX;
				y = offsetY;
			}
		}


		/// <summary>
		/// Returns the transform for this group's coordinate system
		/// </summary>
		/// <returns>The transform.</returns>
		protected Matrix2D ComputeTransform()
		{
			var mat = Matrix2D.Identity;

			if (originX != 0 || originY != 0)
				mat = Matrix2D.Multiply(mat, Matrix2D.CreateTranslation(-originX, -originY));

			if (rotation != 0)
				mat = Matrix2D.Multiply(mat, Matrix2D.CreateRotation(rotation * MathF.PI/180f) );

			if (scaleX != 1 || scaleY != 1)
				mat = Matrix2D.Multiply(mat, Matrix2D.CreateScale(scaleX, scaleY));

			mat = Matrix2D.Multiply(mat, Matrix2D.CreateTranslation(x + originX, y + originY));

			// Find the first parent that transforms
			Group parentGroup = parent;
			while (parentGroup != null)
			{
				if (parentGroup.transform)
					break;

				parentGroup = parentGroup.parent;
			}

			if (parentGroup != null)
				mat = Matrix2D.Multiply(mat, parentGroup.ComputeTransform());

			return mat;
		}

		


		/// <summary>
		/// Set the batch's transformation matrix, often with the result of {@link #computeTransform()}. Note this causes the batch to
		/// be flushed. {@link #resetTransform(Batch)} will restore the transform to what it was before this call.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		/// <param name="transform">Transform.</param>
		protected void ApplyTransform(Matrix transform)
		{
			Rlgl.rlPushMatrix();
			Matrix4x4.Decompose(transform,out Vector3 scale,out Quaternion quaternion,out Vector3 translation);
			Rlgl.rlTranslatef(translation.X,translation.Y,translation.Z);
			Rlgl.rlScalef(scale.X,scale.Y,scale.Z);
		}


		/// <summary>
		/// Restores the batch transform to what it was before {@link #applyTransform(Batch, Matrix4)}. Note this causes the batch to
		/// be flushed
		/// </summary>
		/// <param name="batch">Batch.</param>
		protected void ResetTransform()
		{
			Rlgl.rlPopMatrix();
		}


		/// <summary>
		/// If true, drawDebug() will be called for this group and, optionally, all children recursively.
		/// </summary>
		/// <param name="enabled">If set to <c>true</c> enabled.</param>
		/// <param name="recursively">If set to <c>true</c> recursively.</param>
		public void SetDebug(bool enabled, bool recursively)
		{
			_debug = enabled;
			if (recursively)
			{
				foreach (var child in children)
				{
					if (child is Group)
						((Group)child).SetDebug(enabled, recursively);
					else
						child.SetDebug(enabled);
				}
			}
		}


		/// <summary>
		/// Calls <see cref=" SetDebug(bool, bool)"/>
		/// </summary>
		/// <returns>The all.</returns>
		public virtual Group DebugAll()
		{
			SetDebug(true, true);
			return this;
		}
		public Group ToggleDebugAll()
        {
			var debug = GetDebug();
			if (debug) SetDebug(false, false);
			else SetDebug(true, true);
			return this;
        }


		#region ILayout

		public override bool LayoutEnabled
		{
			get => _layoutEnabled;
			set
			{
				if (_layoutEnabled != value)
				{
					_layoutEnabled = value;

					SetLayoutEnabled(this, _layoutEnabled);
					if (_layoutEnabled)
						InvalidateHierarchy();
				}
			}
		}


		public override void Pack()
		{
			SetSize(PreferredWidth, PreferredHeight);
			Validate();

			// Some situations require another layout. Eg, a wrapped label doesn't know its pref height until it knows its width, so it
			// calls invalidateHierarchy() in layout() if its pref height has changed.
			if (_needsLayout)
			{
				SetSize(PreferredWidth, PreferredHeight);
				Validate();
			}
		}

		#endregion

		public void SetCullingArea(Rectangle cullingArea)
		{
			_cullingArea = cullingArea;
		}
	}
}