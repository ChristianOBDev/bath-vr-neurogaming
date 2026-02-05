# 1. Repository & Project

## 1.1 What goes in the repo

Example repo:

```
📁 Bath-VR-Neurogaming
├── 📁 Unity Project
│   ├── 📁 Assets
│   ├── 📁 Packages
│   └── 📁 Project Settings
├── 📁 Documentation
├── 📄 README.md
└── 📄 .gitignore
```

### Do not put personal Unity experiments in the git repository. Try to keep file sizes down and do not add unnecessary files.

## 1.2 Asset Folder Strructure

Example asset folder:

```
Assets/
├── Art/
│ ├── Animations/
│ ├── Materials/
│ ├── Models/
│ ├── Sprites/
│ └── Textures/
├── Audio/
├── Prefabs/
├── Scenes/
├── Scripts/
│ ├── Gameplay/
│ ├── MotorImagery/
│ ├── MVEP/
│ │ ├── Managers/
│ │ ├── InputHandler/
│ │ ├── Mushrooms/
│ │ ├── Cannon/
│ ├── NeuroFeedback/
│ ├── Passive/
│ ├── ThreatDetection/
│ ├── UI/
│ ├── Systems/
│ └── Utlilites/
├── ScriptableObjects/
├── ThirdParty/
│ └── Plugins/
└ └── Models/
```

**Notes:**

- This list is not exhaustive, use subfolders where necessary.
- Try to keep the structure equal among levels when possible, i.e. A folder contains only subfolders, the subfolders contain files. A folder does not contain both files and subfolders.
- Keep anything Third Party assets in the thirdparty folder. If for some reason you must move the asset out of thirdparty, make sure to leave a note of the license in the thirdparty folder.

## 1.3 Unity Version

It is important we use the same version of Unity. We are using Unity 6000.3.5f2.

# 2. Unity + Git Setup

## 2.1 Unity Editor Settings

All team members must use the same settings:

**Edit → Project Settings → Editor**

Version Control: **Visible Meta Files**

Asset Serialization: **Force Text**

These settings ensure Git can track changes correctly and merge files safely.

# Git Workflow

## 3.1 Branching Strategy

We will utilise a "three branch" strategy:

1. `main` → For stable, playable versions of the game
2. `dev` → For integrating features
3. `feature-<short-name>` → For developing features of the game (probably individually)

**Branches should always be in all lowercase.**

## 3.2 Workflow

We will use a cycle of branching from dev to feature, developing features, integrating on dev, then pushing to main for playable builds

### 3.3 Create a branch

1. `git checkout dev` → Puts you on dev branch
2. `git pull origin dev` → Ensures you are on latest commit with latest changes and integrations
3. `git checkout -b feature-mvep-stimulus` → Creates a local branch from latest `dev` commit, puts you on that branch

### 3.4 Commit to your feature branch

Try to make small focused changes which you can commit frequently. Give yourself as many checkpoints as possible. Commits on `feature` branches are cheap.

```
git status
git add Assets/Scripts/MVEP/Stimuli.cs
git commit -m "Add MVEP stimuli script for use on MVEP prefab"
```

**Please write an informative commit message**

### 3.5 Push your branch

Push your branch to the repo, so that teammembers have access and may inspect

`git push origin feature-mvep-stimuli`

### 3.6 Self-check for merge conflicts

We will try to resolve merge conflicts before they happen on `dev` branch. Before making a pull request to integrate your feature on to `dev`, be sure to pull the latest version of `dev` and merge it into your `feature` branch. You may then resolve merge conflicts locally, before pushing your branch back to the repo.

1. `git fetch origin dev` → Retrieves the latest commit/changes from `dev`
2. `git merge origin dev` → Merges the changes from `dev` into your local `feature` branch
3. `# fix merge conflicts` → Use Unity / IDE / GUI to resolve conflicts. Be sure to test everything in Unity afterwards
4. `# Add merge conflict fixes` → Stage all the changes ytou made in resolving the merge conflicts
5. `git commit -m "Resolve merge conflicts from dev to feature"` → Commit the conflict fix changes to your local branch
6. `git push origin feature-mvep-stimuli` → Push your local branch to the repo again with conflict fixes

### 3.7 Create a pull request

Create a pull request from the Github website. Send a message in the chat, and one or more team members will review the request. If the team thinks the changes look good, the request can be approved and the changes will merge into `dev`. This merge should not create any conflicts, because they have been preemptively resolved on the local branch.

Once the branch has been merged, be sure to get back on the latest commit of `dev`.

```
git checkout dev
git pull origin dev
```

### 3.8 Merging to `main`

When several commits have been made to `dev` and the game is in a playable state, we can merge into `main`. This will be initiated by group discussion and consensus that the state of the game is presentable. The idea is that making a build off of any commit in `main` would result in a playable build.

# 4. Handling Merge Conflicts

## 4.1 Preventative steps

Conflicts will happen, it is inevitable. To reduce the risk of conflicts happening please take these additional steps.

- Work in separate scenes. We can organize to consolidate scenes at a later date.
- Announce that you are working on a scene in the chat/teams channel (we can set this up)
- Break logic into prefabs and scripts

## 4.2 In the case of conflict

- Begin a conversation with the team to clarfiy intent of changes
- Open Unity
- Verify visually
- Test the scene extensively after resolving
- Revert to previous commits if necessary
- Communicate with the team to ensure everyone is aware of changes

# 5. Recap

The key points are:

- Don't bloat the project
- Pull before you start work
- Work on a local branch
- Commit frequently, and always commit before switching tasks
- Merge dev to local feature branch before creating pull request
- Communicate scene ownership
- Do not merge your own pull requests, have the group review
- Communicate and talk through merge conflicts to keep team members informed
