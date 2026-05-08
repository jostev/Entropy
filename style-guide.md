## Unity C# Style Guide
### Comments
Obviously, make your code readable. If you have good variable names, comments aren't needed.

**Only requirement**: at minimum, a single sentence describing each class, function and interface.

### 1. Naming Conventions

| Type | Convention | Example |
| --- | --- | --- |
| **Classes / Structs** | PascalCase | `PlayerController`, `PerkManager` |
| **Public Fields / Properties** | PascalCase | `public float MoveSpeed { get; }` |
| **Private Fields** | _camelCase | `private float _currentHealth;` |
| **Method Names** | PascalCase | `void ApplyPhysicsForce()` |
| **Local Variables** | camelCase | `float distanceToTarget = ...` |
| **Constants** | PASCAL_SNAKE | `const float MAX_GRAVITY = -20f;` |

### 2. Unity Specifics

* **Attributes:** Use `[SerializeField]` to expose private variables to the Inspector instead of making everything `public`.
* **Namespaces:** Wrap your code in a project-specific namespace to avoid collisions with assets.
* **Order of Operations:**
1. Variables (Statics -> Serialized -> Private)
2. Unity Lifecycle (`Awake` -> `OnEnable` -> `Start` -> `Update`)
3. Public Methods
4. Private Methods

> **Jam Rule:** Use `Header` and `Space` attributes to keep your Inspector readable for the rest of the team.
> ```csharp
> [Header("Physics Settings")]
> [SerializeField, Range(0, 10)] private float _friction;
> 
> ```
> 
> 

## Git Commit Style Guide
### Format

`<type>: <description>`

### Types

* **feat:** A new feature (e.g., `feat: add triple jump perk`)
* **fix:** A bug fix (e.g., `fix: player falling through floor in room 3`)
* **refactor:** Code changes that neither fix a bug nor add a feature
* **assets:** Adding models, sounds, or textures (e.g., `assets: import kenney robot models`)
* **tweak:** Adjusting values/balance (e.g., `tweak: reduce enemy fire rate`)
* **docs:** Changes to documentation or README

### Golden Rules for the Jam

1. **Atomic Commits:** Don't commit 10 features at once. If the game breaks, you want to be able to revert just the "Slow-Mo" script, not the entire Player prefab.
2. **Ignore Meta Files:** Ensure your `.gitignore` is set for Unity. Never commit `Library/`, `Temp/`, or `UserSettings/`.
3. **The "Fix" Message:** If a commit fixes a bug, explain *what* was wrong.
* *Bad:* `fix: bug`
* *Good:* `fix: gravity perk not resetting on room transition`

