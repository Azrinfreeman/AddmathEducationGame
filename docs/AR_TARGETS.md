# AR scenes and image targets

This guide maps the eleven enabled AR scenes to their serialized target names. Each scene references `Vuforia/FunMathDatabase.xml`; the names below also appear in the [database definition](../Assets/StreamingAssets/Vuforia/FunMathDatabase.xml). Names and capitalization are preserved, including `perimeter`.

| Scene | Database target |
| --- | --- |
| [ARIsipadu](../Assets/Scenes/AR/ARIsipadu.unity) | `Isipadu` |
| [ARKenaliMinit](../Assets/Scenes/AR/ARKenaliMinit.unity) | `KenaliMinit` |
| [ARLuas](../Assets/Scenes/AR/ARLuas.unity) | `Luas` |
| [ARPerimeter](../Assets/Scenes/AR/ARPerimeter.unity) | `perimeter` |
| [ARPerkaitanWaktu](../Assets/Scenes/AR/ARPerkaitanWaktu.unity) | `PerkaitanWaktu` |
| [ARRekodWaktu](../Assets/Scenes/AR/ARRekodWaktu.unity) | `RekodWaktu` |
| [ARSebutDanTulisWaktu](../Assets/Scenes/AR/ARSebutDanTulisWaktu.unity) | `SebutDanTulisWaktu` |
| [ARApaItuTambah](../Assets/Scenes/AR/Pra/ARApaItuTambah.unity) | `ApaItuOperasiTambah` |
| [AROperasiTambahGunaObjek](../Assets/Scenes/AR/Pra/AROperasiTambahGunaObjek.unity) | `OperasiTambahMenggunakanObjek` |
| [AROperasiTambahGunaJari](../Assets/Scenes/AR/Pra/AROperasiTambahGunaJari.unity) | `OperasiTambahMenggunakanJari` |
| [AROperasiTambahGunaNombor](../Assets/Scenes/AR/Pra/AROperasiTambahGunaNombor.unity) | `OperasiTambahMenggunakanNombor` |

## Included marker cards

The repository contains eleven PNG cards in these folders:

- [PRA SEKOLAH](../Assets/Scenes/AR/06%20KAD%20IMBASAN%20AR/PRA%20SEKOLAH/): four addition cards.
- [TAHAP 1](../Assets/Scenes/AR/06%20KAD%20IMBASAN%20AR/TAHAP%201/): four numbered PNG files.
- [TAHAP 2](../Assets/Scenes/AR/06%20KAD%20IMBASAN%20AR/TAHAP%202/): three numbered PNG files.

The scene-to-target table is verified from serialized fields. Card artwork has not been compared with the database images in this documentation pass; confirm each card against its target before printing or recording a demo. Do not infer a numbered card's target from its filename alone.

## Prepare an AR walkthrough

Follow the README [project setup](../README.md#open-the-project) and [demo checklist](DEMO_CHECKLIST.md) first. Retrieve the required LFS package, use the recorded Unity version, and review the existing Vuforia licence locally. The target XML is an index; retain its paired [database data file](../Assets/StreamingAssets/Vuforia/FunMathDatabase.dat) and the complete project.

On the intended device, check camera permission, target acquisition and loss, displayed content, and return navigation for every scene. This source check did not run Unity, build an app, validate the licence, or test camera tracking. Preserve the existing credits and review card/media usage terms before distributing a demo.
