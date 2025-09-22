# 📚 **مستندات کامل سیستم Unity - Chicken Invaders**

## 📋 **فهرست مطالب**
1. [معرفی پروژه](#معرفی-پروژه)
2. [راه‌اندازی سیستم](#راه-اندازی-سیستم)
3. [ساختار پروژه](#ساختار-پروژه)
4. [سیستم‌های اصلی](#سیستم-های-اصلی)
5. [راهنمای توسعه](#راهنمای-توسعه)
6. [بهینه‌سازی عملکرد](#بهینه-سازی-عملکرد)
7. [راه‌اندازی Build](#راه-اندازی-build)
8. [عیب‌یابی](#عیب-یابی)

---

## 🎯 **معرفی پروژه**

### **نام پروژه:** Chicken Invaders - Unity Edition
### **ورژن:** 1.5.0
### **پلتفرم‌ها:** PC/Mac/Linux, Android/iOS, WebGL
### **نویسنده:** [Your Name]
### **تاریخ:** 2024

### **توضیحات:**
این پروژه یک پیاده‌سازی کامل و حرفه‌ای از بازی کلاسیک Chicken Invaders در Unity است که شامل:
- ✅ **6 نوع اسلحه** با سیستم ارتقا
- ✅ **30 دشمن** در 3 موج با الگوهای حرکت مختلف
- ✅ **سیستم UI کامل** با انیمیشن‌های حرفه‌ای
- ✅ **کنترل‌های لمسی پیشرفته** برای موبایل
- ✅ **سیستم صوتی پویا** با موسیقی تطبیقی
- ✅ **بهینه‌سازی عملکرد** برای دستگاه‌های مختلف

---

## 🚀 **راه‌اندازی سیستم**

### **پیش‌نیازها:**
- **Unity Version:** 2022.3 LTS یا بالاتر
- **Operating System:** Windows 10+, macOS 10.15+, یا Ubuntu 18.04+
- **Hardware:** حداقل 4GB RAM, DirectX 11 یا OpenGL 3.3+

### **مراحل راه‌اندازی:**

#### **1. راه‌اندازی پروژه Unity**
```bash
# 1. کلون کردن پروژه
git clone [repository-url]
cd ChickenInvaders-Unity

# 2. باز کردن در Unity
- Unity Hub را باز کنید
- Add → Existing project را انتخاب کنید
- پوشه پروژه را انتخاب کنید
```

#### **2. تنظیمات اولیه Unity**
```csharp
// Player Settings تنظیمات:
- Company Name: [Your Company]
- Product Name: Chicken Invaders
- Default Icon: Assets/Sprites/Logo.png
- Default Cursor: Assets/Sprites/Cursor.png

// Build Settings:
- Target Platform: PC/Mac/Linux Standalone
- Architecture: x86_64
- Scripting Backend: Mono
```

#### **3. تنظیمات Audio**
```csharp
// Audio Manager Settings:
- Volume: Linear
- Doppler Factor: 1
- Speed of Sound: 343.5
- Default Speaker Mode: Stereo
```

#### **4. تنظیمات Input System**
```csharp
// Input Manager Settings:
- Enable Input System Package (Optional): No
- Use Legacy Input Manager: Yes

// Custom Input Axes:
- Player1_Horizontal: A/D keys
- Player1_Vertical: W/S keys  
- Player1_Fire: Space
- Player2_Horizontal: Left/Right arrows
- Player2_Vertical: Up/Down arrows
- Player2_Fire: Right Shift
```

#### **5. Layer Configuration**
```csharp
// Custom Layers:
Layer 8: Player
Layer 9: Enemy  
Layer 10: Bullet
Layer 11: Pickup
Layer 12: Effects
Layer 13: UI

// Physics2D Layer Collision Matrix:
Player vs Enemy: ✅
Player vs Bullet (Enemy): ✅
Enemy vs Bullet (Player): ✅
Bullet vs Bullet: ❌
UI vs All: ❌
```

#### **6. Quality Settings**
```csharp
// Quality Levels (0-3):
0: Ultra Low - Mobile low-end
1: Low - Mobile mid-range  
2: Medium - Mobile high-end/PC
3: High - PC/Console

// Settings per level:
- Texture Quality: Full/Half/Quarter/Eighth
- Shadow Quality: Disable/Hard Only/All
- Anti Aliasing: None/2x/4x/8x
```

---

## 🏗️ **ساختار پروژه**

### **ساختار فایل‌ها:**
```
Assets/
├── Audio/                          # فایل‌های صوتی
│   ├── Music/                      # موسیقی‌های پس‌زمینه
│   ├── SFX/                        # جلوه‌های صوتی
│   └── Voice/                      # صداهای گفتاری
├── Prefabs/                        # پیش‌ساخته‌ها
│   ├── Player/                     # بازیکن و اسلحه‌ها
│   ├── Enemies/                    # دشمنان
│   ├── UI/                         # رابط کاربری
│   └── Effects/                    # جلوه‌های ویژه
├── Scenes/                         # صحنه‌های بازی
│   ├── MainMenu.unity             # منوی اصلی
│   ├── Gameplay.unity             # بازی اصلی
│   └── Loading.unity              # صحنه بارگذاری
├── ScriptableObjects/              # داده‌های قابل تنظیم
│   ├── Weapons/                   # داده‌های اسلحه
│   ├── Enemies/                   # داده‌های دشمن
│   └── Settings/                  # تنظیمات بازی
├── Scripts/                        # کدهای C#
│   ├── Controllers/               # کنترل‌کننده‌ها
│   ├── Managers/                  # مدیریت‌کننده‌ها
│   ├── UI/                        # رابط کاربری
│   ├── Data/                      # ساختار داده‌ها
│   ├── Effects/                   # جلوه‌های ویژه
│   ├── Utils/                     # ابزارهای کمکی
│   └── Weapons/                   # سیستم اسلحه
├── Sprites/                        # تصاویر و آیکون‌ها
│   ├── Player/                    # تصاویر بازیکن
│   ├── Enemies/                   # تصاویر دشمنان
│   ├── UI/                        # تصاویر رابط
│   ├── Weapons/                   # تصاویر اسلحه
│   └── Effects/                   # تصاویر جلوه‌ها
└── StreamingAssets/               # فایل‌های جریانی
    └── Config/                    # فایل‌های پیکربندی
```

### **Scene Hierarchy:**

#### **MainMenu Scene:**
```
MainMenuScene
├── Canvas (UI)
│   ├── BackgroundImage
│   ├── Logo
│   ├── MainMenuUI
│   │   ├── PlayButton
│   │   ├── SettingsButton
│   │   ├── HighScoresButton
│   │   ├── CreditsButton
│   │   └── QuitButton
│   ├── SettingsUI (Initially Inactive)
│   ├── HighScoreUI (Initially Inactive)
│   └── CreditsUI (Initially Inactive)
├── Audio
│   └── AudioManager
└── EventSystem
```

#### **Gameplay Scene:**
```
GameplayScene
├── Canvas (UI)
│   ├── GameplayHUD
│   ├── PauseMenuUI (Initially Inactive)
│   ├── GameOverUI (Initially Inactive)
│   ├── VictoryUI (Initially Inactive)
│   └── TouchControlsUI (Mobile Only)
├── Cameras
│   └── Main Camera (with CameraController)
├── GameManagement
│   ├── GameManager
│   ├── AudioManager
│   ├── InputManager
│   ├── WeaponManager
│   ├── BulletManager
│   ├── EnemyManager
│   ├── DropManager
│   ├── SaveManager
│   ├── PerformanceOptimizer
│   └── ParticleManager
├── GameObjects
│   ├── Player (Prefab Instance)
│   ├── EnemyContainer
│   ├── BulletContainer
│   └── DropContainer
├── Background
│   ├── BackgroundImage
│   └── ParallaxLayers
└── Effects
    ├── ParticleEffects
    └── LightingEffects
```

---

## ⚙️ **سیستم‌های اصلی**

### **1. Game Management System**

#### **GameManager.cs**
- **مسئولیت:** مدیریت کلی جریان بازی
- **ویژگی‌ها:**
  - Singleton pattern برای دسترسی سراسری
  - مدیریت وضعیت بازی (شروع، توقف، پایان)
  - مدیریت بازیکنان و دشمنان
  - هماهنگی بین سیستم‌های مختلف

```csharp
// استفاده:
GameManager.Instance.StartGame();
GameManager.Instance.PauseGame();
GameManager.Instance.GameOver();
```

#### **SaveManager.cs**
- **مسئولیت:** مدیریت ذخیره‌سازی و بارگذاری
- **ویژگی‌ها:**
  - ذخیره‌سازی تنظیمات کاربر
  - سیستم High Score
  - ذخیره‌سازی آمار بازی
  - رمزنگاری اطلاعات (اختیاری)

### **2. Player System**

#### **PlayerController.cs**
- **مسئولیت:** کنترل بازیکن و رفتارهای آن
- **ویژگی‌ها:**
  - پشتیبانی از 2 بازیکن همزمان
  - سیستم جان و تولد مجدد
  - مدیریت اسلحه و تیراندازی
  - انیمیشن و جلوه‌های بصری

```csharp
// تنظیم کنترل‌ها:
player.SetupControls(PlayerController.ControlScheme.WASD);
player.ChangeWeapon(newWeapon);
player.AddScore(points);
```

### **3. Weapon System**

#### **WeaponManager.cs**
- **مسئولیت:** مدیریت سیستم اسلحه‌ها
- **اسلحه‌های موجود:**
  1. **Ion Blaster** - اسلحه پایه
  2. **Neutron Gun** - تیراندازی دقیق
  3. **Hyper Gun** - ضرر بالا
  4. **Vulcan Chaingun** - تیراندازی سریع
  5. **Plasma Rifle** - پرتو پیوسته
  6. **Lightning Fryer** - پرتو لیزر

#### **سیستم ارتقا:**
- **سطح 1:** تک تیر
- **سطح 2:** دوتیر
- **سطح 3:** سه‌تیر  
- **سطح 4:** چهارتیر

### **4. Enemy System**

#### **EnemyManager.cs**
- **مسئولیت:** مدیریت دشمنان و AI
- **انواع حرکت:**
  1. **Horizontal** - حرکت افقی (موج 1)
  2. **Vertical** - حرکت عمودی (موج 2)
  3. **Spiral** - حرکت مارپیچی (موج 3)

#### **EnemyController.cs**
- **ویژگی‌ها:**
  - AI هوشمند با الگوهای حرکت
  - سیستم سلامت و آسیب
  - Drop system برای آیتم‌ها
  - انیمیشن و جلوه‌های مرگ

### **5. UI System**

#### **UI Components:**
- **MainMenuUI:** منوی اصلی با انیمیشن
- **GameplayHUD:** نمایشگر اطلاعات بازی
- **PauseMenuUI:** منوی توقف با تنظیمات
- **SettingsUI:** تنظیمات کامل صدا/تصویر
- **HighScoreUI:** سیستم امتیازات بالا

### **6. Audio System**

#### **AdvancedAudioManager.cs**
- **ویژگی‌ها:**
  - موسیقی پویا بر اساس وضعیت بازی
  - Audio Mixing حرفه‌ای
  - صوتی فضایی (Spatial Audio)
  - بهینه‌سازی برای موبایل

### **7. Touch Controls**

#### **EnhancedTouchControls.cs**
- **ویژگی‌ها:**
  - جویستیک مجازی responsive
  - Multi-touch support
  - Haptic feedback
  - تطبیق با Safe Area

### **8. Performance System**

#### **PerformanceOptimizer.cs**
- **ویژگی‌ها:**
  - تنظیم خودکار کیفیت
  - Object pooling پیشرفته
  - Culling هوشمند
  - بهینه‌سازی حافظه

---

## 🛠️ **راهنمای توسعه**

### **اضافه کردن اسلحه جدید:**

#### **1. ایجاد WeaponDataSO:**
```csharp
// در Unity Editor:
1. Right Click در Assets/ScriptableObjects/Weapons/
2. Create → Chicken Invaders → Weapon Data
3. تنظیم پروپرتی‌های اسلحه:
   - weaponType: نوع جدید اسلحه
   - weaponName: نام اسلحه
   - baseDamage: خسارت پایه
   - baseFireRate: سرعت شلیک
   - bulletSprite: تصویر گلوله
```

#### **2. اضافه کردن به BulletType enum:**
```csharp
// در BulletManager.cs:
public enum BulletType
{
    Normal,
    Ion,
    Neutron,
    Hyper,
    Vulcan,
    Plasma,
    Laser,
    EnemyEgg,
    NewWeaponType  // ← اضافه کنید
}
```

#### **3. پیاده‌سازی منطق شلیک:**
```csharp
// در WeaponManager.cs → FireWeapon():
case BulletType.NewWeaponType:
    FireNewWeapon(player, weaponData, firePosition, fireDirection);
    break;
```

### **اضافه کردن نوع دشمن جدید:**

#### **1. ایجاد Enemy Prefab:**
```csharp
// مراحل:
1. Duplicate کردن Enemy prefab موجود
2. تغییر Sprite و Animation
3. تنظیم EnemyController parameters
4. اضافه کردن به EnemyManager
```

#### **2. تعریف Movement Pattern:**
```csharp
// در EnemyController.cs:
public enum EnemyMovementType
{
    Horizontal,
    Vertical,
    Spiral,
    NewMovementType  // ← اضافه کنید
}

// پیاده‌سازی در HandleMovement():
case EnemyMovementType.NewMovementType:
    MoveNewPattern();
    break;
```

### **اضافه کردن UI جدید:**

#### **1. ایجاد UI Script:**
```csharp
public class NewUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Button closeButton;
    
    public static NewUI Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void ShowUI()
    {
        panel.SetActive(true);
    }
    
    public void HideUI()
    {
        panel.SetActive(false);
    }
}
```

#### **2. Integration با UIManager:**
```csharp
// در UIManager.cs:
public NewUI newUI;

public void ShowNewUI()
{
    if (newUI != null)
        newUI.ShowUI();
}
```

---

## ⚡ **بهینه‌سازی عملکرد**

### **Object Pooling:**
```csharp
// تمام Bullet ها از Pool استفاده می‌کنند:
BulletManager.Instance.SpawnBullet(position, direction, speed, damage, isPlayer, bulletType);

// تمام Effect ها از Pool استفاده می‌کنند:
ParticleManager.Instance.CreateExplosion(position, ExplosionType.Normal);
```

### **Performance Monitoring:**
```csharp
// Performance Optimizer خودکار کیفیت را تنظیم می‌کند:
PerformanceOptimizer.Instance.SetQualityLevel(2);
PerformanceOptimizer.Instance.EnableAdaptiveQuality(true);

// آمار عملکرد:
float currentFPS = PerformanceOptimizer.Instance.GetCurrentFPS();
float memoryUsage = PerformanceOptimizer.Instance.GetMemoryUsageMB();
```

### **Mobile Optimizations:**
```csharp
// بهینه‌سازی‌های خودکار برای موبایل:
- Texture compression
- Shadow quality reduction
- Particle count limiting
- Audio compression
- Dynamic quality adjustment
```

---

## 📱 **راه‌اندازی Build**

### **PC/Mac/Linux Build:**
```csharp
// Build Settings:
1. File → Build Settings
2. Platform: PC, Mac & Linux Standalone
3. Target Platform: Windows/macOS/Linux
4. Architecture: x86_64
5. Scripting Backend: Mono
6. Api Compatibility Level: .NET Framework
7. Build
```

### **Android Build:**
```csharp
// Prerequisites:
- Android SDK
- Java JDK 8+
- Android NDK (optional)

// Build Settings:
1. Platform: Android
2. Texture Compression: ASTC
3. Minimum API Level: 21 (Android 5.0)
4. Target API Level: Latest
5. Scripting Backend: IL2CPP
6. Target Architectures: ARM64
7. Build Type: Release
```

### **iOS Build:**
```csharp
// Prerequisites:
- Xcode (macOS only)
- iOS Developer Account

// Build Settings:
1. Platform: iOS
2. Target Device: Universal
3. Target iOS Version: 11.0+
4. Scripting Backend: IL2CPP
5. Build
6. Open in Xcode and Archive
```

### **WebGL Build:**
```csharp
// Build Settings:
1. Platform: WebGL
2. Compression Format: Gzip
3. Memory Size: 256MB+
4. Enable Exception Support: None
5. Strip Engine Code: Yes
6. Build
```

---

## 🔧 **عیب‌یابی**

### **مشکلات رایج:**

#### **1. Performance Issues:**
```csharp
// حل:
- بررسی Performance Optimizer logs
- کاهش Particle Quality
- فعال‌سازی Object Pooling
- بررسی Memory Leaks
```

#### **2. Audio Problems:**
```csharp
// حل:
- بررسی Audio Mixer settings
- تست Audio Source configurations
- بررسی Audio Listener conflicts
```

#### **3. Touch Controls Not Working:**
```csharp
// حل:
- بررسی EventSystem در Scene
- تست Canvas Render Mode
- بررسی Touch Input در InputManager
```

#### **4. Build Errors:**
```csharp
// حل:
- بررسی Platform-specific settings
- حذف unused scripts
- بررسی Asset references
```

### **Debug Tools:**

#### **در بازی:**
```csharp
// فعال‌سازی Debug Mode:
GameConstants.DEBUG_MODE = true;

// نمایش Performance Stats:
PerformanceOptimizer.Instance.LogPerformanceReport();

// نمایش FPS Counter:
// در SettingsUI → Show FPS Counter
```

#### **Unity Console:**
```csharp
// Log Types:
[GameManager] - اطلاعات بازی
[AudioManager] - اطلاعات صوتی  
[Performance] - آمار عملکرد
[SaveManager] - عملیات ذخیره‌سازی
```

---

## 📊 **آمار پروژه**

### **کدهای پیاده‌سازی شده:**
- **Scripts:** 25+ فایل C#
- **UI Components:** 8 صفحه
- **Managers:** 7 سیستم مدیریت
- **Controllers:** 3 کنترل‌کننده اصلی
- **Effects:** 4 سیستم جلوه‌های ویژه

### **ویژگی‌های پیاده‌سازی شده:**
- ✅ سیستم 6 اسلحه با ارتقا
- ✅ 30 دشمن در 3 الگوی حرکت
- ✅ UI کامل با انیمیشن
- ✅ کنترل‌های لمسی پیشرفته
- ✅ سیستم صوتی پویا
- ✅ بهینه‌سازی عملکرد
- ✅ پشتیبانی چند پلتفرمه

### **پلتفرم‌های پشتیبانی شده:**
- ✅ Windows/Mac/Linux
- ✅ Android/iOS
- ✅ WebGL

---

## 📞 **پشتیبانی و ارتباط**

### **منابع اضافی:**
- [Unity Documentation](https://docs.unity3d.com/)
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Mobile Optimization Guide](https://docs.unity3d.com/Manual/MobileOptimisation.html)

### **مشارکت در پروژه:**
1. Fork کردن repository
2. ایجاد branch جدید
3. Commit تغییرات
4. ایجاد Pull Request

---

**آخرین به‌روزرسانی:** دسامبر 2024  
**ورژن مستندات:** 1.5.0  
**وضعیت پروژه:** آماده برای Build و انتشار ✅