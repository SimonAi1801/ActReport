using ActReport.Core.Contracts;
using ActReport.Core.Entities;
using ActReport.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ActReport.ViewModel
{
    public class EmployeeViewModel : BaseViewModel
    {
        private string _firstName;
        private string _lastName;
        private string _filterText = "A";
        private Employee _selectedEmployee;
        private ObservableCollection<Employee> _employees;
        private ICommand _cmdSaveChanges;
        private ICommand _cmdNewEmp;

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                FirstName = _selectedEmployee?.FirstName;
                LastName = _selectedEmployee?.LastName;
                OnPropertyChanged(nameof(SelectedEmployee));
            }
        }
        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged(nameof(Employee));
            }
        }
        public ICommand CmdSaveChanges
        {
            get
            {
                if (_cmdSaveChanges == null)
                {
                    _cmdSaveChanges = new RelayCommand(
                        execute: _ =>
                        {
                            using IUnitOfWork uow = new UnitOfWork();
                            _selectedEmployee.FirstName = _firstName;
                            _selectedEmployee.LastName = _lastName;
                            uow.EmployeeRepository.Update(_selectedEmployee);
                            uow.Save();

                            LoadEmployees();
                        },
                        canExecute: _ => _selectedEmployee != null && _lastName.Length >= 3);
                }

                return _cmdSaveChanges;
            }

            set { _cmdSaveChanges = value; }
        }

        public ICommand CmdSaveNewEmp
        {
            get
            {
                if (_cmdNewEmp == null)
                {
                    _cmdNewEmp = new RelayCommand(
                        execute: _ =>
                        {
                            using IUnitOfWork uow = new UnitOfWork();
                            Employee newEmployee = new Employee
                            {
                                FirstName = _firstName,
                                LastName = _lastName
                            };
                            uow.EmployeeRepository.Insert(newEmployee);
                            uow.Save();

                            LoadEmployees();
                        },
                        canExecute: _ => _firstName != null && _lastName?.Length >= 3);
                }
                return _cmdNewEmp;
            }
            set { _cmdNewEmp = value; }
        }

        public EmployeeViewModel()
        {
            LoadEmployees();
        }
        private void LoadEmployees()
        {
            using (UnitOfWork uow = new UnitOfWork())
            {
                var employees = uow.EmployeeRepository
                    .Get(orderBy: coll => coll.OrderBy(emp => emp.LastName),
                         filter: emp => emp.LastName.StartsWith(FilterText))
                    .ToList();

                Employees = new ObservableCollection<Employee>(employees);
            }
        }

        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set 
            { 
                _filterText = value;

                LoadEmployees();
            }
        }
    }
}
