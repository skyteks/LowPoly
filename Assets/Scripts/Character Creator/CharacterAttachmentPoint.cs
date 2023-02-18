using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttachmentPoint : MonoBehaviour
{
    public enum AttachmentTypes
    {
        HeadCoverings_Base,
        HeadCoverings_No_FacialHair,
        HeadCoverings_No_Hair,
        Hair,
        HelmetAttachment,
        BackAttachment,
        ShoulderAttachmentRight,
        ShoulderAttachmentLeft,
        ElbowAttachmentRight,
        ElbowAttachmentLeft,
        HipsAttachment,
        KneeAttachmentRight,
        KneeAttachmentLeft,
        Ear,

        Head_Male,
        Head_No_Elements_Male,
        Eyebrow_Male,
        FacialHair_Male,
        Torso_Male,
        ArmUpperRight_Male,
        ArmUpperLeft_Male,
        ArmLowerRight_Male,
        ArmLowerLeft_Male,
        HandRight_Male,
        HandLeft_Male,
        Hips_Male,
        LegRight_Male,
        LegLeft_Male,

        Head_Female,
        Head_No_Elements_Female,
        Eyebrow_Female,
        FacialHair_Female,
        Torso_Female,
        ArmUpperRight_Female,
        ArmUpperLeft_Female,
        ArmLowerRight_Female,
        ArmLowerLeft_Female,
        HandRight_Female,
        HandLeft_Female,
        Hips_Female,
        LegRight_Female,
        LegLeft_Female,
    }

    public enum RootBoneNames
    {
        Spine_03,
        Head,
        Back_Attachment,
        Shoulder_Attachment_R,
        Shoulder_Attachment_L,
        Elbow_Attachment_R,
        Elbow_Attachment_L,
        Hips_Attachment,
        Knee_Attachment_R,
        Knee_Attachment_L,
        Neck,
        Eyebrows,
        Hips,
        Clavicle_R,
        Clavicle_L,
        Shoulder_R,
        Shoulder_L,
        Hand_R,
        Hand_L,
        LowerLeg_R,
        LowerLeg_L,
    }

    [SerializeField]
    private AttachmentTypes attachmentType;
    [SerializeField]
    private RootBoneNames rootBoneName;
}
