import setuptools
import shutil
import os
from distutils.command.build_py import build_py

# Prepare list of binaries and package version

data = os.getenv('NLedgerPackageBuildBinaries', '').split(';')
ver = os.getenv('NLedgerPackageBuildVersion', '')

if not data:
    raise Exception("No binary files specified; environment variable NLedgerPackage.Build.Binaries must contain semicolon-separated list of files.")

if not ver:
    raise Exception("Version is not specified; environment variable NLedgerPackage.Build.Version must contain a package version.")

# Custom build extension

class ledger_build_py(build_py):
    def run(self):
        # honor the --dry-run flag
        if not self.dry_run:

            target_dir = os.path.join(self.build_lib, 'ledger/runtime')
            self.mkpath(target_dir)

            for datafile in data:
                shutil.copy(datafile, target_dir) 

        # distutils uses old-style classes, so no super()
        build_py.run(self)

# Build process

setuptools.setup(
  version=ver,
  cmdclass={'build_py': ledger_build_py}
)