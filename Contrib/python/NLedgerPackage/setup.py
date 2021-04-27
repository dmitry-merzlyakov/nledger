import setuptools

import os
from distutils.command.build_py import build_py

# Prepare dotnet binaries

from subprocess import Popen, PIPE
import shutil

psfile = os.path.join(os.path.dirname(os.path.realpath(__file__)), 'setup_runtime.ps1')
process = Popen(['powershell.exe', '-NoLogo', '-ExecutionPolicy', 'RemoteSigned', '-File', psfile], stdout=PIPE)
(output, err) = process.communicate()
exit_code = process.wait()

if exit_code > 0:
    raise Exception("Dotnet build script failed")

data = output.splitlines()
ver = data.pop(0).decode("utf-8")

print (data)
print (data[0].decode("utf-8"))

# Custom build extension

class my_build_py(build_py):
    def run(self):
        # honor the --dry-run flag
        if not self.dry_run:

            target_dir = os.path.join(self.build_lib, 'mypytest/runtime')
            self.mkpath(target_dir)

            for datafile in data:
                shutil.copy(datafile.decode("utf-8"), target_dir) 

        # distutils uses old-style classes, so no super()
        build_py.run(self)

# Build process

setuptools.setup(
  version=ver,
  cmdclass={'build_py': my_build_py}
)