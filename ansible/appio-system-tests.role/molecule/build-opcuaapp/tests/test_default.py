import pytest
import os

import testinfra.utils.ansible_runner

testinfra_hosts = testinfra.utils.ansible_runner.AnsibleRunner(
    os.environ['MOLECULE_INVENTORY_FILE']
).get_hosts('all')


def prepare_provide_test_directory(host, case):
    test_dir_path = case + '/'

    mk_test_dir = host.run('mkdir --parents ' + test_dir_path)

    assert mk_test_dir.rc == 0

    test_dir = host.file(test_dir_path)

    assert test_dir.exists
    assert test_dir.is_directory

    return test_dir_path


@pytest.mark.parametrize('case, command', [
    ['1', 'appio build --name my-app'],
    ['2', 'appio build -n     my-app'],
])
def test_that_appio_build_opcuaapp_is_succeeding(host, case, command):
    # prepare
    test_dir_path = prepare_provide_test_directory(host, case)

    log_file_path = test_dir_path + 'appio.log'
    client_app_exe_file_path = test_dir_path + 'my-app/build/client-app'
    server_app_exe_file_path = test_dir_path + 'my-app/build/server-app'

    for prepare_command in (
        'appio new opcuaapp -n my-app -t ClientServer -u 127.0.0.1 -p 4840',
        'rm -f appio.log',
    ):
        prepare = host.run('cd ' + test_dir_path + ' && ' + prepare_command)

        assert prepare.rc == 0

    # arrange
    log_file = host.file(log_file_path)
    assert not log_file.exists

    # act
    appio = host.run('cd ' + test_dir_path + ' && ' + command)

    # assert
    assert appio.rc == 0
    assert appio.stdout != ''

    log_file = host.file(log_file_path)
    assert log_file.exists

    client_app_exe_file = host.file(client_app_exe_file_path)
    assert client_app_exe_file.exists
    assert client_app_exe_file.mode == 0o755

    server_app_exe_file = host.file(server_app_exe_file_path)
    assert server_app_exe_file.exists
    assert server_app_exe_file.mode == 0o755


@pytest.mark.parametrize('case, command', [
    ['1f', 'appio build --name my-app-5263452364'],
    ['2f', 'appio build -n     my-app-5263452364'],
    ['3f', 'appio build --name my/-app'],
    ['4f', 'appio build -n     my/-app'],
    ['5f', 'appio build --name'],
    ['6f', 'appio build -n'],
    ['7f', 'appio build --exit'],
    ['8f', 'appio build -x'],
])
def test_that_appio_build_opcuaapp_is_failing(host, case, command):
    # prepare
    test_dir_path = prepare_provide_test_directory(host, case)

    log_file_path = test_dir_path + 'appio.log'

    # arrange
    log_file = host.file(log_file_path)
    assert not log_file.exists

    # act
    appio = host.run('cd ' + test_dir_path + ' && ' + command)

    # assert
    assert appio.rc != 0
    assert appio.stdout != ''

    log_file = host.file(log_file_path)
    assert log_file.exists


@pytest.mark.parametrize('case, command', [
    ['1f_meson', 'appio build --name my-app'],
])
def test_that_appio_build_opcuaapp_is_failing_when_meson_call_fails(host, case, command):  # noqa: #501
    # prepare
    test_dir_path = prepare_provide_test_directory(host, case)

    log_file_path = test_dir_path + 'appio.log'

    for prepare_command in (
        'appio new opcuaapp -n my-app -t ClientServer -u 127.0.0.1 -p 4840',
        'rm -f my-app/meson.build',
        'rm -f appio.log',
    ):
        prepare = host.run('cd ' + test_dir_path + ' && ' + prepare_command)

        assert prepare.rc == 0

    # arrange
    log_file = host.file(log_file_path)
    assert not log_file.exists

    # act
    appio = host.run('cd ' + test_dir_path + ' && ' + command)

    # assert
    assert appio.rc != 0
    assert appio.stdout != ''

    log_file = host.file(log_file_path)
    assert log_file.exists


@pytest.mark.parametrize('case, command', [
    ['1f_ninja', 'appio build --name my-app'],
])
def test_that_appio_build_opcuaapp_is_failing_when_ninja_call_fails(host, case, command):  # noqa: #501
    # prepare
    test_dir_path = prepare_provide_test_directory(host, case)

    log_file_path = test_dir_path + 'appio.log'

    for prepare_command in (
        'appio new opcuaapp -n my-app -t ClientServer -u 127.0.0.1 -p 4840',
        'rm -f my-app/src/server/main.c',
        'rm -f appio.log',
    ):
        prepare = host.run('cd ' + test_dir_path + ' && ' + prepare_command)

        assert prepare.rc == 0

    # arrange
    log_file = host.file(log_file_path)
    assert not log_file.exists

    # act
    appio = host.run('cd ' + test_dir_path + ' && ' + command)

    # assert
    assert appio.rc != 0
    assert appio.stdout != ''

    log_file = host.file(log_file_path)
    assert log_file.exists