
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG_IPHONESIMULATOR)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/slingr.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

SLINGR_DLL_MDB_SOURCE=bin/Debug/slingr.dll.mdb
SLINGR_DLL_MDB=$(BUILD_DIR)/slingr.dll.mdb

endif

ifeq ($(CONFIG),RELEASE_IPHONESIMULATOR)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/slingr.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

SLINGR_DLL_MDB=

endif

ifeq ($(CONFIG),DEBUG_IPHONE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/slingr.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

SLINGR_DLL_MDB_SOURCE=bin/Debug/slingr.dll.mdb
SLINGR_DLL_MDB=$(BUILD_DIR)/slingr.dll.mdb

endif

ifeq ($(CONFIG),RELEASE_IPHONE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/slingr.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

SLINGR_DLL_MDB=

endif

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/slingr.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

SLINGR_DLL_MDB_SOURCE=bin/Debug/slingr.dll.mdb
SLINGR_DLL_MDB=$(BUILD_DIR)/slingr.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/slingr.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

SLINGR_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SLINGR_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SLINGR_PC)  


RESGEN=resgen2

SLINGR_PC = $(BUILD_DIR)/slingr.pc

FILES = \
	LingrClient.cs \
	System.Json/JsonArray.cs \
	System.Json/JsonObject.cs \
	System.Json/JsonPrimitive.cs \
	System.Json/JsonReader.cs \
	System.Json/JsonType.cs \
	System.Json/JsonValue.cs \
	System.Json/MergedEnumerable.cs \
	System/UriTemplate.cs \
	System/UriTemplateEquivalenceComparer.cs \
	System/UriTemplateMatch.cs \
	System/UriTemplateMatchException.cs \
	System/UriTemplateTable.cs \
	MonoTODOAttribute.cs \
	System.Runtime.Serialization.Json/DataContractJsonSerializer.cs \
	System.Runtime.Serialization.Json/IXmlJsonReaderInitializer.cs \
	System.Runtime.Serialization.Json/IXmlJsonWriterInitializer.cs \
	System.Runtime.Serialization.Json/JsonReader.cs \
	System.Runtime.Serialization.Json/JsonReaderWriterFactory.cs \
	System.Runtime.Serialization.Json/JsonSerializationReader.cs \
	System.Runtime.Serialization.Json/JsonSerializationWriter.cs \
	System.Runtime.Serialization.Json/JsonWriter.cs \
	System.Runtime.Serialization.Json/TypeMap.cs \
	LingrContractTypes.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	slingr.pc.in 

REFERENCES =  \
	System \
	System.Xml \
	System.Core \
	System.Runtime.Serialization \
	System.ServiceModel.Web

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-wrapper,SLINGR_PC,slingr.pc))


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
	$(call cp,$(SLINGR_DLL_MDB),$(DESTDIR)$(libdir)\$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)\pkgconfig'
	$(call cp,$(SLINGR_PC),$(DESTDIR)$(libdir)\pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SLINGR_DLL_MDB),$(DESTDIR)$(libdir)\$(PACKAGE))
	$(call rm,$(SLINGR_PC),$(DESTDIR)$(libdir)\pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
