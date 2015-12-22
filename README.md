# Duplicate File Finder

Simple OOP demo for search of duplicated files


### I. General description, main features

Main focus in this project was made on simplification of extensibility. Wpf application able to use custom file providers like Azure (currently in development), Amazon S3, FTP and other's.
Beside that there also possibility to add custom comparators and select order in which they will be applied to files.


### II. Comparators

Comparison process splitted in two stages:
* Calculation of comparation criteria, any calculations with single file (ex. size, hash).
* Comparation of calculated criterias. any actions with two files (ex. explisit comparation).

Currently application contains those comparators:
* `SizeComparator` fastest and usually first comparator base on files size;
* `HashComparator` comparation base on calculating file hash (Sha-512 by default, put can be configured), hash comparation faster that excplicit only in case of comparation more that two files;
* `ExplicitComparator` slowest comparator base on direst comparation of file content.


### III. Solution structure

#### 1. General interfaces 
`DuplicateFileFinder.Core.Interfaces` - contains base intarfaces and could be used for creation of custom file providers `IFileProvider` and comparators `IFileComparator`.

#### 2. Application core
`DuplicateFileFinder.Core` - contains main logic of appliction:
* `ComparationManager` class that manages file comparation, could work with any providers and comparators that implements spesial interfaces. 
Support cancellation based on `CancellationToken` and reporting about current progress.
* `LocalFileProvider` allow work with files in local file system.
* Namespace `DuplicateFileFinder.Core.Comparators` contains file comparators:
* Namespace `DuplicateFileFinder.Core.ProgressChanges` contains classes that are used for tracking comparation progress.
* `ComparationSettings` low level compation settings like buffer size and default hash algorithm.

#### 3. Console demo application
`DuplicateFileFinder.Core.Console` - simplest console interface for file comparation.

#### 4. WPF application for usage of comparation core
`DuplicateFileFinder.Core.UI` Allow to add file providers, comparators and change order of comparators applying.

#### 5. Unit test for comparation core.
`DuplicateFileFinder.Core.Tests`


#### 6. Class libary for implementation of Azure Blob file provider (Currently in development).
`DuplicateFileFinder.Core.Azure`


#### 7. Univarsal application comparation core
`DuplicateFileFinder.Core.Universal` - classes that allow compartion core to work in Universal Windows Applications, 
could be used but for this part of `DuplicateFileFinder.Core` should be moved to shared project, which has some minor difficalties in configuration.


### IV. Performance optimization

#### 1. Redundant comparator call's
Hash comparation for two files meanless, would be better to call explicit comparation directly.

#### 2. There are not always need in complete hash calculation. 
Hash comparator can be splited into couple parts like calculation of first 100 Mb, next 1GB and rest part of file. 
Comparing of those parts and not complete file hash could save a lot of time, and in some cases be more precise than regular hash comparison.

#### 3. Some comparison actions could be applied before previous finished. 
For example, comparison of creteries could be started as soon as we have two criterias, not all of them. But this approach will require much more work in multi thread access synchronization.
