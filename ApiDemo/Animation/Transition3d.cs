/*
 * Copyright (C) 2007 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Views.Animations;
using Java.Lang;

namespace MonoDroid.ApiDemo
{
	public class Transition3d : Activity
	{
		private ListView mPhotosList;
		private ViewGroup mContainer;
		private ImageView mImageView;

		public Transition3d (IntPtr handle)
			: base (handle)
		{
		}

		// Names of the photos we show in the list
		private static string[] PHOTOS_NAMES = new String[] {
			"Lyon",
			"Livermore",
			"Tahoe Pier",
			"Lake Tahoe",
			"Grand Canyon",
			"Bodie"
		};

		// Resource identifiers for the photos we want to display
		private static int[] PHOTOS_RESOURCES = new int[] {
			R.drawable.photo1,
			R.drawable.photo2,
			R.drawable.photo3,
			R.drawable.photo4,
			R.drawable.photo5,
			R.drawable.photo6
		};

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (R.layout.animations_main_screen);

			mPhotosList = (ListView)FindViewById (Android.R.Id.List);
			mImageView = (ImageView)FindViewById (R.id.picture);
			mContainer = (ViewGroup)FindViewById (R.id.container);

			// Prepare the ListView
			ArrayAdapter<String> adapter = new ArrayAdapter<String> (this,
				Android.R.Layout.SimpleListItem1, PHOTOS_NAMES);

			mPhotosList.Adapter = adapter;
			mPhotosList.ItemClick += OnItemClick;

			// Prepare the ImageView
			mImageView.Clickable = true;
			mImageView.Focusable = true;
			mImageView.Click += OnClick;
			//mImageView.SetOnClickListener (this);

			// Since we are caching large views, we want to keep their cache
			// between each animation
			mContainer.PersistentDrawingCache = ViewGroup.PersistentAnimationCache;
		}

		/**
 * Setup a new 3D rotation on the container view.
 *
 * @param position the item that was clicked to show a picture, or -1 to show the list
 * @param start the start angle at which the rotation must begin
 * @param end the end angle of the rotation
 */
		private void ApplyRotation (int position, float start, float end)
		{
			// Find the center of the container
			float centerX = mContainer.Width / 2.0f;
			float centerY = mContainer.Height / 2.0f;

			// Create a new 3D rotation with the supplied parameter
			// The animation listener is used to trigger the next animation
			Rotate3dAnimation rotation =
				new Rotate3dAnimation (start, end, centerX, centerY, 310.0f, true);
			rotation.Duration = 500;
			rotation.FillAfter = true;
			rotation.Interpolator = new AccelerateInterpolator ();
			rotation.SetAnimationListener (new DisplayNextView (position, mContainer, mPhotosList, mImageView));

			mContainer.StartAnimation (rotation);
		}


		/**
	     * This class listens for the end of the first half of the animation.
	     * It then posts a new action that effectively swaps the views when the container
	     * is rotated 90 degrees and thus invisible.
	     */
		private class DisplayNextView : Animation.IAnimationListener
		{
			private int position;
			private ViewGroup container;
			private ListView photos_list;
			private ImageView image_view;

			public DisplayNextView (int position, ViewGroup container, ListView photosList, ImageView imageView)
			{
				this.position = position;
				this.container = container;
				photos_list = photosList;
				image_view = imageView;
			}


			#region IAnimationListener Members
			public void OnAnimationEnd (Animation animation)
			{
				container.Post (new SwapViews (position, container, photos_list, image_view));
			}

			public void OnAnimationRepeat (Animation animation)
			{
			}

			public void OnAnimationStart (Animation animation)
			{
			}
			#endregion

			#region IJavaObject Members

			public IntPtr Handle
			{
				get { throw new NotImplementedException (); }
			}

			#endregion
		}

		/**
	     * This class is responsible for swapping the views and start the second
	     * half of the animation.
	     */
		private class SwapViews : IRunnable
		{
			private int position;
			private ViewGroup container;
			private ListView photos_list;
			private ImageView image_view;

			public SwapViews (int position, ViewGroup container, ListView photosList, ImageView imageView)
			{
				this.position = position;
				this.container = container;
				photos_list = photosList;
				image_view = imageView;
			}

			public void Run ()
			{
				float centerX = container.Width / 2.0f;
				float centerY = container.Height / 2.0f;
				Rotate3dAnimation rotation;

				if (position > -1) {
					photos_list.Visibility = View.Gone;
					image_view.Visibility = View.Visible;
					image_view.RequestFocus ();

					rotation = new Rotate3dAnimation (90, 180, centerX, centerY, 310.0f, false);
				} else {
					image_view.Visibility = View.Gone;
					photos_list.Visibility = View.Visible;
					photos_list.RequestFocus ();

					rotation = new Rotate3dAnimation (90, 0, centerX, centerY, 310.0f, false);
				}

				rotation.Duration = 500;
				rotation.FillAfter = true;
				rotation.Interpolator = new DecelerateInterpolator ();

				container.StartAnimation (rotation);
			}


			#region IJavaObject Members

			public IntPtr Handle
			{
				get { throw new NotImplementedException (); }
			}

			#endregion
		}


		#region IOnItemClickListener Members

		public void OnItemClick (AdapterView<object> parent, View view, int position, long id)
		{
			// Pre-load the image then start the animation
			mImageView.SetImageResource (PHOTOS_RESOURCES[position]);
			ApplyRotation (position, 0, 90);
		}

		#endregion

		#region IOnClickListener Members

		public void OnClick (View v)
		{
			ApplyRotation (-1, 180, 90);
		}

		#endregion
	}
}