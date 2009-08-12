.PHONY : all build test clean

export TOP = $(CURDIR)
export D_OUT = $(TOP)/bin/Debug

export CSC = gmcs
export CSFLAGS = -debug -warn:2 -warnaserror
export MONO = mono

NUNIT_FLAGS = -nologo -noshadow

export LIBPATH = $(D_OUT)
export NUNIT = nunit-console2 $(NUNIT_FLAGS)

MAKE_DIR = $(MAKE) -C 

LIB_DIR = ManagedOpenSsl
TEST_DIR = test/UnitTests

all: build

lib: 
	$(MAKE_DIR) $(LIB_DIR)

build: lib build_test 

build_test:
	$(MAKE_DIR) $(TEST_DIR)

test: lib build_test 
	$(MAKE_DIR) $(TEST_DIR) test

clean: clean_lib clean_test

clean_lib:
	$(MAKE_DIR) $(LIB_DIR) clean

clean_test:
	$(MAKE_DIR) $(TEST_DIR) clean

install:
	echo install

pkg:
	echo pkg

dist:
	echo disto

publish:
	echo publish

release:
	echo release
