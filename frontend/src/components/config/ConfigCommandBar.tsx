import {
    ActionButton,
    CommandBar,
    ContextualMenu,
    DefaultButton,
    Dialog,
    DialogFooter,
    DialogType,
    ICommandBarItemProps,
    IDialogContentProps,
    IModalProps,
    ITextField,
    PrimaryButton,
    TextField
} from "@fluentui/react";
import { useBoolean } from '@fluentui/react-hooks';

export interface ConfigCommandBarProps {
    addConfirmButtonText: string;
    addDialogHidden: boolean;
    addNameRef: React.RefObject<ITextField>;
    deleteButtonDisabled: boolean;
    deleteDialogHidden: boolean;
    hideAddDialog: () => void;
    hideDeleteDialog: () => void;
    onAddConfirmClick: () => void;
    onDeleteConfirmClick: () => void;
    pluralItemName: string;
    showAddDialog: () => void;
    showDeleteDialog: () => void;
    singularItemName: string;
}

const draggableProps: IModalProps = {
    dragOptions: {
        moveMenuItemText: 'Move',
        closeMenuItemText: 'Close',
        menu: ContextualMenu
    }
}

export function ConfigCommandBar(props: ConfigCommandBarProps): JSX.Element {
    const {
        addConfirmButtonText,
        addDialogHidden,
        addNameRef,
        deleteButtonDisabled,
        deleteDialogHidden,
        hideAddDialog,
        hideDeleteDialog,
        onAddConfirmClick,
        onDeleteConfirmClick,
        pluralItemName,
        showAddDialog,
        showDeleteDialog,
        singularItemName
    } = props;

    const commandBarItems: ICommandBarItemProps[] = [
        {
            key: 'add',
            text: 'Add',
            iconProps: { iconName: 'Add' },
            onClick: () => showAddDialog()
        },
        {
            key: 'delete',
            text: 'Delete',
            iconProps: { iconName: 'Delete' },
            ariaDescription: `Delete selected ${pluralItemName}`,
            disabled: deleteButtonDisabled,
            onClick: () => showDeleteDialog()
        }
    ]

    const addContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: `Create ${singularItemName}`,
        subText: `Please specify the name of the ${singularItemName} to be created.`
    }

    const deleteConfirmProps: IDialogContentProps = {
        type: DialogType.normal,
        title: `Delete ${pluralItemName}`,
        subText: `Are you sure you want to delete the selected ${pluralItemName}?`
    }

    return (
        <>
            <CommandBar
                items={commandBarItems} />
            <Dialog
                hidden={addDialogHidden}
                modalProps={draggableProps}
                dialogContentProps={addContentProps}
                onDismiss={hideAddDialog}>
                <TextField label={"Name"} componentRef={addNameRef}/>
                <DialogFooter>
                    <DefaultButton onClick={onAddConfirmClick} text={addConfirmButtonText}/>
                    <ActionButton onClick={hideAddDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
            <Dialog
                hidden={deleteDialogHidden}
                modalProps={draggableProps}
                dialogContentProps={deleteConfirmProps}
                onDismiss={hideDeleteDialog} >
                <DialogFooter>
                    <PrimaryButton onClick={onDeleteConfirmClick} text="Delete" />
                    <DefaultButton onClick={hideDeleteDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
        </>
    )
}
