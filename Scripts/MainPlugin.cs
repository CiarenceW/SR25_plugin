using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Receiver2ModdingKit;
using Receiver2;
using RewiredConsts;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Security.Cryptography;

namespace SR25_plugin
{
    public class SR25_Script : ModGunScript
    {
        private float hammer_accel = -5000;
        private float m_charging_handle_amount;
        private ModHelpEntry help_entry;
        public Transform dust_cover_component;
        private RotateMover dust_cover = new RotateMover();
        private bool dust_cover_opened;
        private Spring pose_ads_spring = new Spring(0, 0, 90, 1E-06f);
        public Transform pose_backup;
        public Transform pose_scope;
        private Transform pose_ads;
        public GameObject linza; //TIL linza is russian for "lens"
        private bool player_scoped = true;
        private ReceiverCoreScript RCS;
        private readonly float[] slide_push_hammer_curve = new float[] {
            0f,
            0f,
            0.35f,
            1f
        };
        LocalAimHandler lah = LocalAimHandler.player_instance;
        public override ModHelpEntry GetGunHelpEntry()
        {
            return help_entry = new ModHelpEntry("SR-25")
            {
                info_sprite = spawn_info_sprite,
                title = "KAC SR-25",
                description = "Knight's Armament Company Stoner Rifle 25\n"
                            + "Capacity: 20 + 1, 7.62 Nato\n"
                            + "\n"
                            + "The SR25 is a semi-automatic rifle that was developed by Eugene Stoner. It is loosely based on the AR-10, also designed by Stoner, whilst still being 60% interchangeable with the AR-15, again, also designed by Stoner. The design team aimed to create a rifle that could serve as a designated marksman rifle, meaning it could accurately engage targets at ranges beyond what a typical infantry rifle could handle. The SR25 was also designed to be modular, so that it could be easily adapted to different situations and environments.\n"
                            + "\n"
                            + "Thanks to these qualities, it has been used extensively by both military and law enforcement agencies around the world. Its accuracy, reliability, and modular design have made it a popular choice for snipers and designated marksmen. It has seen combat in a number of conflicts the Afghanistan War, and the Iraq War. In addition to military and law enforcement use, the SR25 has also been adopted by some civilian shooters for hunting and long-range target shooting.\n"
            };
        }
        public override LocaleTactics GetGunTactics()
        {
            return new LocaleTactics()
            {
                gun_internal_name = InternalName,
                title = "KAC SR-25\n",
                text = "A modded DMR, made by repeatedly banging my head on a wall\n" +
                       "A 7.62 Nato DMR designed by Eugene Stoner, and based upon his earlier designs. The name is a combination of his initials and the numbers obtained by adding those of the AR-10 and AR-15 together.\n"
            };
        }
        public override CartridgeSpec GetCustomCartridgeSpec()
        {
            return new CartridgeSpec()
            {
                extra_mass = 22.4f,
                mass = 3f,
                speed = 833f,
                diameter = 0.00782f
            };
        }
        public override void InitializeGun()
        {
            RCS = ReceiverCoreScript.Instance();
            pooled_muzzle_flash = ((GunScript)RCS.generic_prefabs.First(it => { return it is GunScript && ((GunScript)it).gun_model == GunModel.Deagle; })).pooled_muzzle_flash;
            //loaded_cartridge_prefab = ((GunScript)ReceiverCoreScript.Instance().generic_prefabs.First(it => { return it is GunScript && ((GunScript)it).gun_model == GunModel.Glock; })).loaded_cartridge_prefab;
            RCS.GetMagazinePrefab("Ciarencew.SR25", MagazineClass.LowCapacity).glint_renderer.material = RCS.GetMagazinePrefab("wolfire.glock_17", MagazineClass.StandardCapacity).glint_renderer.material;
            RCS.GetMagazinePrefab("Ciarencew.SR25", MagazineClass.StandardCapacity).glint_renderer.material = RCS.GetMagazinePrefab("wolfire.glock_17", MagazineClass.StandardCapacity).glint_renderer.material;
        }
        public override void AwakeGun()
        {
            hammer.amount = 1f;

            dust_cover.transform = dust_cover_component;

            pose_ads = transform.Find("pose_aim_down_sights");

            dust_cover.rotations[0] = transform.Find("dust_cover_closed").localRotation;
            dust_cover.rotations[1] = transform.Find("dust_cover_opened").localRotation;
        }

        public override void UpdateGun()
        {
            pose_ads_spring.Update(Time.deltaTime); //updates the pose spring

            //pose spring to smoothly switch between sights
            this.pose_ads.localRotation = Quaternion.LerpUnclamped(pose_scope.localRotation, pose_backup.localRotation, pose_ads_spring.state); 
            this.pose_ads.localPosition = Vector3.LerpUnclamped(pose_scope.localPosition, pose_backup.localPosition, pose_ads_spring.state); 

            if (lah.IsHoldingGun) //is the player holding the gun? (not in inventory)
            {
                if (player_input.GetButtonDown(Action.Hammer) && lah.IsAiming()) //is the player holding the hammer key and aiming
                {
                    ToggleActiveSight(); //changes the active sight
                }
                linza.SetActive(true); //enables the lens' gameobject
            }
            else
            {
                linza.SetActive(false); //disables the lens gameobject, preventing the scope from working (otherwise it would still zoom in, no matter the angle, which looked weird.)
            }

            hammer.asleep = true;
            hammer.accel = hammer_accel;

            if (slide.amount > 0 && _hammer_state != 3)
            { // Bolt cocks the hammer when moving back 
                hammer.amount = Mathf.Max(hammer.amount, InterpCurve(slide_push_hammer_curve, slide.amount));
            }

            if (hammer.amount == 1) _hammer_state = 3;

            if (IsSafetyOn())
            {
                trigger.amount = Mathf.Min(trigger.amount, 0.1f);

                trigger.UpdateDisplay();
            }

            if (hammer.amount == 0 && _hammer_state == 2)
            { // If hammer dropped and hammer was cocked then fire gun and decock hammer
                TryFireBullet(1, FireBullet);

                _hammer_state = 0;

                _disconnector_needs_reset = true;
            }

            if (trigger.amount == 0 && slide.amount == 0)
            {
                _disconnector_needs_reset = false;
            }

            if (slide_stop.amount == 1)
            {
                slide_stop.asleep = true;
            }

            if (slide.amount == 0 && _hammer_state == 3 && _disconnector_needs_reset == false)
            { // Simulate auto sear
                hammer.amount = Mathf.MoveTowards(hammer.amount, _hammer_cocked_val, Time.deltaTime * Time.timeScale * 50);
                if (hammer.amount == _hammer_cocked_val) _hammer_state = 2;
            }

            if (_hammer_state != 3 && ((trigger.amount == 1 && !_disconnector_needs_reset && slide.amount == 0) || hammer.amount != _hammer_cocked_val))
            {
                hammer.asleep = false;
            }

            hammer.TimeStep(Time.deltaTime);

            if (player_input.GetButton(Action.Pull_Back_Slide) || player_input.GetButtonUp(Action.Pull_Back_Slide))
            {
                m_charging_handle_amount = Mathf.MoveTowards(m_charging_handle_amount, slide.amount, Time.deltaTime * 20f / Time.timeScale);
            }
            else
            {
                m_charging_handle_amount = Mathf.MoveTowards(m_charging_handle_amount, 0, Time.deltaTime * 50f);
            }

            ApplyTransform("charging_handle", m_charging_handle_amount, transform.Find("charging_handle"));
            ApplyTransform("charging_handle_latch", m_charging_handle_amount, transform.Find("charging_handle/charging_handle_latch"));

            if ((!lah.IsAiming() && player_input.GetButtonDown(Action.Hammer)) || (!dust_cover_opened && slide.amount > 0.03f)) //dust cover opening/closing logic
            {
                ToggleDustCover();
            }

            dust_cover.UpdateDisplay();
            dust_cover.TimeStep(Time.deltaTime);

            hammer.UpdateDisplay();

            slide_stop.UpdateDisplay();

            UpdateAnimatedComponents();
        }
        private void ToggleActiveSight()
        {
            if (player_scoped) //is the player scoped?
            {
                pose_ads_spring.target_state = 1f;
                player_scoped = false;
            }
            else
            {
                pose_ads_spring.target_state = 0f;
                player_scoped = true;
            }
        }
        private void ToggleDustCover()
        {
            dust_cover.asleep = false;
            if (dust_cover.target_amount == 1f && slide.amount <= 0.03f)
            {
                dust_cover.target_amount = 0f;
                dust_cover.accel = -1f;
                dust_cover.vel = -10f;
                AudioManager.PlayOneShotAttached(sound_cylinder_close, dust_cover.transform.gameObject);
                dust_cover_opened = false;
            }
            else if (dust_cover_opened == false)
            {
                dust_cover.target_amount = 1f;
                dust_cover.accel = 1;
                dust_cover.vel = 10;
                AudioManager.PlayOneShotAttached(sound_cylinder_open, dust_cover.transform.gameObject);
                dust_cover_opened = true;
            }
        }
    }
}
