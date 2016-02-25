////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Changes a target GameObject's material in response to hover or selection events from the stylus.
/// </summary>
public class MaterialChanger : MonoBehaviour
{
  public Renderer Target;

  public Material BaseMaterial;
  public Material SelectedMaterial;
  public Material HoveredMaterial;
  public Material SelectedAndHoveredMaterial;

  protected bool _isHovered;
  protected bool _isSelected;

  void OnSelected()
  {
    _isSelected = true;
    RefreshMaterial();
  }

  void OnDeselected()
  {
    _isSelected = false;
    RefreshMaterial();
  }

  void OhHovered()
  {
    _isHovered = true;
    RefreshMaterial();
  }

  void OnUnhovered()
  {
    _isHovered = false;
    RefreshMaterial();
  }

  void RefreshMaterial()
  {
    if (Target == null)
      return;

    Material newMaterial = null;
    if (!_isSelected && !_isHovered)
      newMaterial = BaseMaterial;
    else if (_isSelected && !_isHovered)
      newMaterial = SelectedMaterial;
    else if (!_isSelected && _isHovered)
      newMaterial = HoveredMaterial;
    else if (_isSelected && _isHovered)
      newMaterial = SelectedAndHoveredMaterial;

    if (newMaterial != null)
      Target.material = newMaterial;
  }

  void Start()
  {
    if (Target == null)
      return;

    if (BaseMaterial == null)
      BaseMaterial = Target.material;

    if (SelectedAndHoveredMaterial == null)
      SelectedAndHoveredMaterial = HoveredMaterial;
  }
}
