Yes, with Git Annex, you can achieve this by using the `git annex init --shared` option when initializing the second clone. This will allow both clones to share the same annex data store.

Here's a step-by-step guide:

1. Clone the repository for the first time and initialize Git Annex as you normally would:


```bash
git clone <repository-url>
cd <repository-name>
git annex init
```

2. Clone the repository for the second time in a different directory:

```bash
git clone <repository-url> <repository-name-2>
cd <repository-name-2>
```

3. Initialize Git Annex in the second clone, specifying the path to the first clone's annex data store:\

```bash
git annex init --shared /path/to/first/clone/.git/annex
```
   Replace `/path/to/first/clone/.git/annex` with the actual path to the first clone's annex data store.

By doing this, both clones will share the same annex data store, and any data added to the annex in one clone will be available in the other clone as well.

Note that you can also use `git annex init --shared=group` to specify a group name for the shared annex data store. This allows you to easily manage multiple clones that share the same annex data store.

For example:
   ```bash
git annex init --shared=group=mygroup
```
   Then, in the second clone:
   ```bash
git annex init --shared=group=mygroup
```
   This way, you can easily manage multiple clones that share the same annex data store, without having to specify the path to the annex data store manually.

## Links

* [Git Annex Tutorial](https://github.com/emanuele/git-annex_tutorial) contains information on init --shared.

