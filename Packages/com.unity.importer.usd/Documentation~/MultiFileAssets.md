# Multi-File Assets

Unity's support for Multi-file USD assets allows you to import assets composed from multiple layers, where the layers are stored in separate files.

Using multi-file assets makes it easier for multiple artists to collaborate on the same assets and scenes, and provides smaller change deltas when committing changes to these files in a version control system (VCS). See [Why use USD?](https://openusd.org/release/intro.html#why-use-usd) for more detail.

## Import multi-file assets

To import a multi-file USD asset, add all the files to your Unity project, then enable the **Import** setting on the **root file** only of your composite USD asset. The other files are imported automatically by the USD importer if the root file correctly references them.

When you import a multi-file asset, all the assets contained in each of the child files become sub-assets of the root file asset. 

## Notes for exporting your multi-file assets

- When exporting your USD stage as multiple files, you must specify that the root file refers to the child files using a **relative path**.

- You must ensure that all the USD assets in a given composition exist in the assets folder. You cannot add some files from a multi-file composition and omit others.

- If you have more than one multi-file composition in your project, it's possible for more than one root file to reference the same child file. 
  
  If you do this, the shared child file will not be a shared reference. Instead it becomes duplicated into each root asset. 

  Here Root file A and Root file E both reference child file D, which means the assets inside File D are duplicated as sub assets inside the import result for both A and E.
