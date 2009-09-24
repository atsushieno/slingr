
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG_IPHONESIMULATOR)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/slingr-radar.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../slingr/bin/Debug/slingr.dll
BUILD_DIR = bin/Debug

LINGR_ICO_SOURCE=lingr.ico
SLINGR_DLL_SOURCE=../slingr/bin/Debug/slingr.dll
SLINGR_RADAR_EXE_MDB_SOURCE=bin/Debug/slingr-radar.exe.mdb
SLINGR_RADAR_EXE_MDB=$(BUILD_DIR)/slingr-radar.exe.mdb

endif

ifeq ($(CONFIG),RELEASE_IPHONESIMULATOR)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/slingr-radar.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../slingr/bin/Release/slingr.dll
BUILD_DIR = bin/Release

LINGR_ICO_SOURCE=lingr.ico
SLINGR_DLL_SOURCE=../slingr/bin/Release/slingr.dll
SLINGR_RADAR_EXE_MDB=

endif

ifeq ($(CONFIG),DEBUG_IPHONE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/slingr-radar.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../slingr/bin/Debug/slingr.dll
BUILD_DIR = bin/Debug

LINGR_ICO_SOURCE=lingr.ico
SLINGR_DLL_SOURCE=../slingr/bin/Debug/slingr.dll
SLINGR_RADAR_EXE_MDB_SOURCE=bin/Debug/slingr-radar.exe.mdb
SLINGR_RADAR_EXE_MDB=$(BUILD_DIR)/slingr-radar.exe.mdb

endif

ifeq ($(CONFIG),RELEASE_IPHONE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/slingr-radar.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../slingr/bin/Release/slingr.dll
BUILD_DIR = bin/Release

LINGR_ICO_SOURCE=lingr.ico
SLINGR_DLL_SOURCE=../slingr/bin/Release/slingr.dll
SLINGR_RADAR_EXE_MDB=

endif

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/slingr-radar.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../slingr/bin/Debug/slingr.dll
BUILD_DIR = bin/Debug

LINGR_ICO_SOURCE=lingr.ico
SLINGR_DLL_SOURCE=../slingr/bin/Debug/slingr.dll
SLINGR_RADAR_EXE_MDB_SOURCE=bin/Debug/slingr-radar.exe.mdb
SLINGR_RADAR_EXE_MDB=$(BUILD_DIR)/slingr-radar.exe.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/slingr-radar.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../slingr/bin/Release/slingr.dll
BUILD_DIR = bin/Release

LINGR_ICO_SOURCE=lingr.ico
SLINGR_DLL_SOURCE=../slingr/bin/Release/slingr.dll
SLINGR_RADAR_EXE_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(LINGR_ICO) \
	$(SLINGR_DLL) \
	$(SLINGR_RADAR_EXE_MDB)  

BINARIES = \
	$(SLINGR_RADAR)  


RESGEN=resgen2

LINGR_ICO = $(BUILD_DIR)/lingr.ico
SLINGR_DLL = $(BUILD_DIR)/slingr.dll
SLINGR_RADAR = $(BUILD_DIR)/slingr-radar

FILES = \
	SlingrRadar.cs 

DATA_FILES = 

RESOURCES = \
	lingr.ico 

EXTRAS = \
	slingr-radar.in 

REFERENCES =  \
	System.Windows.Forms \
	System.Drawing \
	System.Core \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,LINGR_ICO))
$(eval $(call emit-deploy-target,SLINGR_DLL))
$(eval $(call emit-deploy-wrapper,SLINGR_RADAR,slingr-radar,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


$(ASSEMBLY_MDB): $(ASSEMBLY)
$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	make install-satellite-assemblies prefix=$(prefix)
	mkdir -p '$(DESTDIR)$(libdir)/$(PACKAGE)'
	$(call cp,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)\$(PACKAGE)'
	$(call cp,$(LINGR_ICO),$(DESTDIR)$(libdir)\$(PACKAGE))
	$(call cp,$(SLINGR_DLL),$(DESTDIR)$(libdir)\$(PACKAGE))
	$(call cp,$(SLINGR_RADAR_EXE_MDB),$(DESTDIR)$(libdir)\$(PACKAGE))
	mkdir -p '$(DESTDIR)$(bindir)'
	$(call cp,$(SLINGR_RADAR),$(DESTDIR)$(bindir))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LINGR_ICO),$(DESTDIR)$(libdir)\$(PACKAGE))
	$(call rm,$(SLINGR_DLL),$(DESTDIR)$(libdir)\$(PACKAGE))
	$(call rm,$(SLINGR_RADAR_EXE_MDB),$(DESTDIR)$(libdir)\$(PACKAGE))
	$(call rm,$(SLINGR_RADAR),$(DESTDIR)$(bindir))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
