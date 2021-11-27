# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.1.0 (2021-11-27)

### Changed

- If the working directory differs from the applications base directory, then the serilog configuration file will be copied from the application directory to the working directory (if the file not already exists). This helps with isolating different configuration based on the working directory of an application.
___

## 1.0.0 (2021-10-15)

Initial release.