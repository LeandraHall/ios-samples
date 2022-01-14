﻿/*
See LICENSE folder for this sample’s licensing information.

Abstract:
Pinned sction headers example
*/

namespace Conference_Diffable.CompositionalLayout.BasicsViewControllers;

public partial class PinnedSectionHeaderFooterViewController : UIViewController, IUICollectionViewDelegate {
	static readonly string SectionHeaderElementKind = nameof (SectionHeaderElementKind);
	static readonly string SectionFooterElementKind = nameof (SectionFooterElementKind);

	UICollectionViewDiffableDataSource<NSNumber, NSNumber>? dataSource;
	UICollectionView? collectionView;

	public override void ViewDidLoad ()
	{
		base.ViewDidLoad ();
		// Perform any additional setup after loading the view, typically from a nib.

		NavigationItem.Title = "Pinned Section Headers";
		ConfigureHierarchy ();
		ConfigureDataSource ();
	}

	UICollectionViewLayout CreateLayout ()
	{
		var itemSize = NSCollectionLayoutSize.Create (NSCollectionLayoutDimension.CreateFractionalWidth (1),
			NSCollectionLayoutDimension.CreateFractionalHeight (1));
		var item = NSCollectionLayoutItem.Create (itemSize);

		var groupSize = NSCollectionLayoutSize.Create (NSCollectionLayoutDimension.CreateFractionalWidth (1),
			NSCollectionLayoutDimension.CreateAbsolute (44));
		var group = NSCollectionLayoutGroup.CreateHorizontal (groupSize, item);

		var section = NSCollectionLayoutSection.Create (group);
		section.InterGroupSpacing = 5;
		section.ContentInsets = new NSDirectionalEdgeInsets (0, 10, 0, 10);

		var headerFooterSize = NSCollectionLayoutSize.Create (NSCollectionLayoutDimension.CreateFractionalWidth (1),
			NSCollectionLayoutDimension.CreateAbsolute (44));
		var sectionHeader = NSCollectionLayoutBoundarySupplementaryItem.Create (headerFooterSize, SectionHeaderElementKind, NSRectAlignment.Top);
		var sectionFooter = NSCollectionLayoutBoundarySupplementaryItem.Create (headerFooterSize, SectionFooterElementKind, NSRectAlignment.Bottom);
		sectionHeader.PinToVisibleBounds = true;
		sectionHeader.ZIndex = 2;
		section.BoundarySupplementaryItems = new [] { sectionHeader, sectionFooter };

		var layout = new UICollectionViewCompositionalLayout (section);
		return layout;
	}

	void ConfigureHierarchy ()
	{
		if (View is null)
			throw new InvalidOperationException (nameof (View));

		collectionView = new UICollectionView (View.Bounds, CreateLayout ()) {
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
			BackgroundColor = UIColor.SystemBackgroundColor,
			Delegate = this
		};
		collectionView.RegisterClassForCell (typeof (ListCell), ListCell.Key);
		collectionView.RegisterClassForSupplementaryView (typeof (TitleSupplementaryView), new NSString (SectionHeaderElementKind), TitleSupplementaryView.Key);
		collectionView.RegisterClassForSupplementaryView (typeof (TitleSupplementaryView), new NSString (SectionFooterElementKind), TitleSupplementaryView.Key);
		View.AddSubview (collectionView);
	}

	void ConfigureDataSource ()
	{
		if (collectionView is null)
			throw new InvalidOperationException (nameof (collectionView));

		dataSource = new UICollectionViewDiffableDataSource<NSNumber, NSNumber> (collectionView, CellProviderHandler) {
			SupplementaryViewProvider = SupplementaryViewProviderHandler
		};

		// initial data
		var itemsPerSection = 5;
		var sections = Enumerable.Range (0, 5).Select (i => NSNumber.FromInt32 (i));
		var snapshot = new NSDiffableDataSourceSnapshot<NSNumber, NSNumber> ();
		var itemOffset = 0;

		foreach (var section in sections) {
			snapshot.AppendSections (new [] { section });
			var items = Enumerable.Range (itemOffset, itemsPerSection).Select (i => NSNumber.FromInt32 (i)).ToArray ();
			snapshot.AppendItems (items);
			itemOffset += itemsPerSection;
		}

		dataSource.ApplySnapshot (snapshot, false);

		UICollectionViewCell CellProviderHandler (UICollectionView collectionView, NSIndexPath indexPath, NSObject obj)
		{
			// Get a cell of the desired kind.
			if (collectionView.DequeueReusableCell (ListCell.Key, indexPath) is ListCell cell) {
				// Populate the cell with our item description.
				cell.Label.Text = $"{indexPath.Section}, {indexPath.Row}";

				// Return the cell.
				return cell;
			}
			throw new InvalidOperationException ("UICollectionViewCell");
		}

		UICollectionReusableView SupplementaryViewProviderHandler (UICollectionView collectionView, string kind, NSIndexPath indexPath)
		{
			// Get a supplementary view of the desired kind.
			if (collectionView.DequeueReusableSupplementaryView (new NSString (kind), TitleSupplementaryView.Key, indexPath) is TitleSupplementaryView supplementaryView) {
				// Populate the view with our section's  description.
				var viewKind = kind == SectionHeaderElementKind ? "Header" : "Footer";

				supplementaryView.Label.Text = $"{viewKind} for section {indexPath.Section}";
				supplementaryView.BackgroundColor = UIColor.LightGray;
				supplementaryView.Layer.BorderColor = UIColor.Black.CGColor;
				supplementaryView.Layer.BorderWidth = 1;

				// Return the view.
				return supplementaryView;
			}
			throw new InvalidOperationException ("UICollectionReusableView");
		}
	}

	#region UICollectionView Delegate

	[Export ("collectionView:didSelectItemAtIndexPath:")]
	public void ItemSelected (UICollectionView collectionView, NSIndexPath indexPath)
		=> collectionView.DeselectItem (indexPath, true);

	#endregion
}
