﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;


public class SkeletalJoint
{
    public CustomRigidGroup childGroup;
    public CustomRigidGroup parentGroup;
    public CustomRigidJoint rigidJoint;
    public AssemblyJointDefinition asmJoint;
    public AssemblyJoint asmJointOccurrence;

    public bool childIsTheOne;


    public SkeletalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (rigidJoint.joints.Count != 1)
            throw new Exception("Not a proper joint");

        asmJoint = rigidJoint.joints[0].Definition;
        asmJointOccurrence = rigidJoint.joints[0];
        childGroup = null;
        parentGroup = parent;
        this.rigidJoint = rigidJoint;
        if (rigidJoint.groupOne.Equals(parent))
        {
            childGroup = rigidJoint.groupTwo;
        }
        else if (rigidJoint.groupTwo.Equals(parent))
        {
            childGroup = rigidJoint.groupOne;
        }
        else
        {
            throw new Exception("Couldn't match parent group");
        }
        if (childGroup == null)
            throw new Exception("Not a proper joint: No child joint found");

        childIsTheOne = childGroup.Contains(asmJointOccurrence.AffectedOccurrenceOne);
        if (!childIsTheOne && !childGroup.Contains(asmJointOccurrence.AffectedOccurrenceTwo))
        {
            throw new Exception("Expected child not found inside assembly joint.");
        }
    }

    public CustomRigidGroup GetChild()
    {
        return childGroup;
    }

    public CustomRigidGroup GetParent()
    {
        return parentGroup;
    }

    public void DoHighlight()
    {
        ComponentHighlighter.PrepareHighlight();
        ComponentHighlighter.ClearHighlight();
        foreach (ComponentOccurrence child in childGroup.occurrences)
        {
            ComponentHighlighter.CHILD_HIGHLIGHT_SET.AddItem(child);
        }
        foreach (ComponentOccurrence parent in parentGroup.occurrences)
        {
            ComponentHighlighter.PARENT_HIGHLIGHT_SET.AddItem(parent);
        }
    }

    public static SkeletalJoint_Base Create(CustomRigidJoint rigidJoint, CustomRigidGroup parent)
    {
        if (RotationalJoint.IsRotationalJoint(rigidJoint))
            return new RotationalJoint(parent, rigidJoint);
        if (LinearJoint.IsLinearJoint(rigidJoint))
            return new LinearJoint(parent, rigidJoint);
        if (CylindricalJoint.IsCylindricalJoint(rigidJoint))
            return new CylindricalJoint(parent, rigidJoint);
        if (PlanarJoint.IsPlanarJoint(rigidJoint))
            return new PlanarJoint(parent, rigidJoint);
        if(BallJoint.IsBallJoint(rigidJoint))
            return new BallJoint(parent, rigidJoint);
        if(RigidJoint.IsRigidJoint(rigidJoint))
            return new RigidJoint(parent, rigidJoint);
        return null;
    }
}